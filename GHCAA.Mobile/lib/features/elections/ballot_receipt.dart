import 'dart:typed_data';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'election_service.dart';

const ballotAbstainedLabel = 'Abstained';

// One line per position: the names picked, or "Abstained". Used by the review step and the receipt.
List<({String position, String choice})> ballotReceiptLines(
    List<BallotSeat> seats, Map<int, List<int>> choices) {
  return [
    for (final seat in seats)
      (
        position: seat.positionTitle,
        choice: () {
          final names = [
            for (final id in (choices[seat.seatId] ?? const <int>[]))
              ...seat.candidates
                  .where((c) => c.nominationId == id)
                  .map((c) => c.name)
          ];
          return names.isEmpty ? ballotAbstainedLabel : names.join(', ');
        }(),
      ),
  ];
}

// Built from what the voter just confirmed. Nothing here is saved or sent anywhere.
Future<Uint8List> buildBallotReceiptPdf({
  required String electionTitle,
  required String dateText,
  required String trackingCode,
  required List<({String position, String choice})> lines,
}) async {
  final pdf = pw.Document();
  pdf.addPage(
    pw.Page(
      pageFormat: PdfPageFormat.a4,
      build: (context) => pw.Column(
        crossAxisAlignment: pw.CrossAxisAlignment.start,
        children: [
          pw.Text(electionTitle,
              style:
                  pw.TextStyle(fontSize: 20, fontWeight: pw.FontWeight.bold)),
          pw.SizedBox(height: 6),
          pw.Text('Voting receipt, $dateText'),
          pw.SizedBox(height: 16),
          pw.Text('Tracking code',
              style: const pw.TextStyle(fontSize: 10, color: PdfColors.grey700)),
          pw.Text(trackingCode,
              style:
                  pw.TextStyle(fontSize: 18, fontWeight: pw.FontWeight.bold)),
          pw.SizedBox(height: 20),
          pw.Divider(),
          for (final line in lines)
            pw.Padding(
              padding: const pw.EdgeInsets.symmetric(vertical: 4),
              child: pw.Row(
                crossAxisAlignment: pw.CrossAxisAlignment.start,
                children: [
                  pw.Expanded(
                      flex: 2,
                      child: pw.Text(line.position,
                          style:
                              pw.TextStyle(fontWeight: pw.FontWeight.bold))),
                  pw.Expanded(flex: 3, child: pw.Text(line.choice)),
                ],
              ),
            ),
          pw.Divider(),
          pw.SizedBox(height: 8),
          pw.Text(
              'The tracking code shows the ballot was counted. It is published without choices.',
              style: const pw.TextStyle(fontSize: 9, color: PdfColors.grey700)),
        ],
      ),
    ),
  );
  return pdf.save();
}
