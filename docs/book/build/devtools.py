"""A small DevTools Protocol client, so the PDF can carry page numbers.

Standard library only: a WebSocket client in about a hundred lines, which is
cheaper than the alternative. Chrome's `--print-to-pdf` command-line switch
prints without a page number and offers no way to add one; it also drops
background graphics. Both are settable on `Page.printToPDF` over the protocol,
along with a header and footer template, so that is the route used.

Chrome still does not implement CSS margin boxes (`@page { @bottom-center }`),
which is how a print stylesheet would normally place a folio. The header and
footer templates below are the only mechanism available, and they apply the
same markup to every page: one page-number sequence for the whole document,
no per-chapter running head. Where the book needs more than that, it is
recorded as a deviation rather than faked.
"""

import base64
import json
import os
import re
import shutil
import socket
import struct
import subprocess
import tempfile
import time
import urllib.request


class ProtocolError(Exception):
    pass


# --------------------------------------------------------------------------
# websocket, just enough of RFC 6455 to talk to one local Chrome
# --------------------------------------------------------------------------

class WebSocket(object):
    def __init__(self, url, timeout=120):
        match = re.match(r"ws://([^:/]+):(\d+)(/.*)", url)
        if not match:
            raise ProtocolError("cannot parse %s" % url)
        host, port, path = match.group(1), int(match.group(2)), match.group(3)
        self.sock = socket.create_connection((host, port), timeout=timeout)
        key = base64.b64encode(os.urandom(16)).decode()
        self.sock.sendall(
            ("GET %s HTTP/1.1\r\nHost: %s:%d\r\nUpgrade: websocket\r\n"
             "Connection: Upgrade\r\nSec-WebSocket-Key: %s\r\n"
             "Sec-WebSocket-Version: 13\r\n\r\n" % (path, host, port, key)).encode())
        data = self._read_until(b"\r\n\r\n")
        header, _, rest = data.partition(b"\r\n\r\n")
        if b"101" not in header.split(b"\r\n")[0]:
            self.sock.close()
            raise ProtocolError("websocket upgrade refused: %r" % header[:200])
        # Whatever arrived in the same read as the header is the start of the
        # first frame. Discarding it leaves the next recv mid-frame.
        self.buffer = rest

    def _read_until(self, marker):
        data = b""
        while marker not in data:
            chunk = self.sock.recv(4096)
            if not chunk:
                raise ProtocolError("connection closed during handshake")
            data += chunk
        return data

    def send(self, text):
        payload = text.encode("utf-8")
        header = bytearray([0x81])          # FIN + text frame
        mask = os.urandom(4)
        length = len(payload)
        if length < 126:
            header.append(0x80 | length)
        elif length < 1 << 16:
            header.append(0x80 | 126)
            header += struct.pack(">H", length)
        else:
            header.append(0x80 | 127)
            header += struct.pack(">Q", length)
        header += mask
        masked = bytes(b ^ mask[i % 4] for i, b in enumerate(payload))
        self.sock.sendall(bytes(header) + masked)

    def _recv_exact(self, count):
        while len(self.buffer) < count:
            chunk = self.sock.recv(65536)
            if not chunk:
                raise ProtocolError("connection closed")
            self.buffer += chunk
        data, self.buffer = self.buffer[:count], self.buffer[count:]
        return data

    def recv(self):
        """One text message. Continuation frames are joined; control frames answered."""
        parts = []
        while True:
            first, second = self._recv_exact(2)
            fin, opcode = first & 0x80, first & 0x0F
            length = second & 0x7F
            if length == 126:
                length = struct.unpack(">H", self._recv_exact(2))[0]
            elif length == 127:
                length = struct.unpack(">Q", self._recv_exact(8))[0]
            payload = self._recv_exact(length) if length else b""
            if opcode == 0x9:                       # ping
                self.sock.sendall(b"\x8a" + bytes([len(payload)]) + payload)
                continue
            if opcode == 0x8:                       # close
                raise ProtocolError("browser closed the connection")
            parts.append(payload)
            if fin:
                return b"".join(parts).decode("utf-8", "replace")

    def close(self):
        try:
            self.sock.sendall(b"\x88\x00")
        except OSError:
            pass
        self.sock.close()


# --------------------------------------------------------------------------
# chrome
# --------------------------------------------------------------------------

class Browser(object):
    """A headless Chrome or Edge with the protocol port open."""

    def __init__(self, executable, timeout=180):
        self.timeout = timeout
        self.profile = tempfile.mkdtemp(prefix="book-cdp-")
        self.proc = subprocess.Popen([
            executable,
            "--headless=new",
            "--disable-gpu",
            "--no-sandbox",
            "--no-first-run",
            "--no-default-browser-check",
            "--disable-extensions",
            "--disable-dev-shm-usage",
            "--remote-debugging-port=0",
            "--user-data-dir=%s" % self.profile,
            "--window-size=1240,1754",
            # Deliberately without printer.py's
            # --run-all-compositor-stages-before-draw and
            # --virtual-time-budget: those make a one-shot --dump-dom run wait
            # for Mermaid, and on a long-lived session the virtual-time budget
            # kills the page target instead. This session waits explicitly, on
            # document.body.dataset.diagrams, which is the stronger check.
            "about:blank",
        ], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        # Anything raised from here on escapes the constructor, so `with
        # Browser(...)` never runs __exit__: the browser would stay alive and
        # the profile directory would accumulate.
        try:
            self.port = self._wait_for_port()
            self.ws = WebSocket(self._page_target(), timeout=timeout)
        except BaseException:
            self.close()
            raise
        self.next_id = 0

    def _wait_for_port(self):
        marker = os.path.join(self.profile, "DevToolsActivePort")
        deadline = time.time() + 45
        while time.time() < deadline:
            if os.path.exists(marker):
                text = open(marker).read().split("\n")
                if text and text[0].strip().isdigit():
                    return int(text[0].strip())
            if self.proc.poll() is not None:
                raise ProtocolError("the browser exited before opening its port")
            time.sleep(0.1)
        raise ProtocolError("the browser did not open a protocol port")

    def _page_target(self):
        deadline = time.time() + 30
        while time.time() < deadline:
            try:
                raw = urllib.request.urlopen(
                    "http://127.0.0.1:%d/json/list" % self.port, timeout=5).read()
                for target in json.loads(raw):
                    if target.get("type") == "page" and target.get("webSocketDebuggerUrl"):
                        return target["webSocketDebuggerUrl"]
            except Exception:
                pass
            time.sleep(0.2)
        raise ProtocolError("no page target to attach to")

    def call(self, method, **params):
        self.next_id += 1
        message_id = self.next_id
        self.ws.send(json.dumps({"id": message_id, "method": method, "params": params}))
        deadline = time.time() + self.timeout
        while time.time() < deadline:
            message = json.loads(self.ws.recv())
            if message.get("id") != message_id:
                continue                            # an event, or another call
            if "error" in message:
                raise ProtocolError("%s: %s" % (method, message["error"].get("message")))
            return message.get("result", {})
        raise ProtocolError("%s did not answer within %ds" % (method, self.timeout))

    def open_page(self, url):
        self.call("Page.enable")
        self.call("Page.navigate", url=url)

    def wait_for(self, expression, seconds=120, poll=0.5):
        """Poll a JavaScript expression until it is true."""
        deadline = time.time() + seconds
        while time.time() < deadline:
            result = self.call("Runtime.evaluate", expression=expression, returnByValue=True)
            if result.get("result", {}).get("value"):
                return True
            time.sleep(poll)
        return False

    def evaluate(self, expression):
        result = self.call("Runtime.evaluate", expression=expression, returnByValue=True)
        return result.get("result", {}).get("value")

    def print_pdf(self, out_path, header="", footer="", margins_mm=(20, 18, 22, 18)):
        top, right, bottom, left = [mm / 25.4 for mm in margins_mm]
        result = self.call(
            "Page.printToPDF",
            printBackground=True,
            preferCSSPageSize=True,            # keeps @page A4 and the landscape page
            displayHeaderFooter=bool(header or footer),
            headerTemplate=header or "<span></span>",
            footerTemplate=footer or "<span></span>",
            marginTop=top, marginRight=right, marginBottom=bottom, marginLeft=left,
            transferMode="ReturnAsBase64",
        )
        data = result.get("data")
        if not data:
            raise ProtocolError("printToPDF returned no data")
        with open(out_path, "wb") as fh:
            fh.write(base64.b64decode(data))
        return out_path

    def close(self):
        # Also called from the constructor's failure path, where the socket may
        # never have been made.
        try:
            if getattr(self, "ws", None) is not None:
                self.ws.close()
        except OSError:
            pass
        finally:
            self.proc.terminate()
            try:
                self.proc.wait(timeout=10)
            except subprocess.TimeoutExpired:
                self.proc.kill()
            shutil.rmtree(self.profile, ignore_errors=True)

    def __enter__(self):
        return self

    def __exit__(self, *exc):
        self.close()
