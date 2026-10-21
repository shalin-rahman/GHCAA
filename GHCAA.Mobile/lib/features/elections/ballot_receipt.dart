import 'election_service.dart';

const ballotAbstainedLabel = 'Abstained';

// One line per position: the names picked, or "Abstained". Used by the review step.
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
