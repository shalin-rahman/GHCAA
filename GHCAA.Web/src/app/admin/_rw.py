def rw(p,f):
    b=open(p,'rb').read().decode()
    crlf='\r\n' in b
    t=b.replace('\r\n','\n')
    t2=f(t)
    assert t2!=t,p
    if crlf:t2=t2.replace('\n','\r\n')
    open(p,'wb').write(t2.encode())
def sub(t,a,b,count=1):
    assert a in t,a
    return t.replace(a,b) if count==0 else t.replace(a,b,count)
