class AcademicConstants {
  static const List<String> certificates = [
    'HSC',
    'Bachelor (Pass)',
    'Bachelor (Honours)',
    'Masters',
    'PGD',
    'PhD',
    'Medicine',
    'Engineering',
    'Law'
  ];

  static const List<String> hscSubjects = [
    'Science',
    'Arts & Humanities',
    'Business Studies'
  ];

  static const List<String> generalSubjects = [
    'Bengali', 'English', 'History', 'Islamic History & Culture',
    'Philosophy', 'Islamic Studies', 'Library Science', 'Economics',
    'Political Science', 'Sociology', 'Social Work', 'Anthropology',
    'Public Administration', 'Physics', 'Chemistry', 'Mathematics',
    'Statistics', 'Botany', 'Zoology', 'Geography & Environment',
    'Psychology', 'Soil Science', 'Accounting', 'Management',
    'Marketing', 'Finance & Banking', 'Fine Arts', 'Physical Education',
    'Business Administration', 'Computer', 'Civil', 'Mechanical',
    'Electrical', 'Medical', 'Dentestry', 'Engineering', 'Law',
    'Pharma', 'Agriculture', 'Textile', 'Lather', 'Education'
  ];

  static List<int> getAcademicYears() {
    final int currentYear = DateTime.now().year;
    const int startYear = 1950;
    return List.generate(currentYear - startYear + 1, (i) => currentYear - i);
  }
}

class MembershipConstants {
  static const List<Map<String, String>> typeOptions = [
    {'value': 'Founding', 'label': 'Founding Member'},
    {'value': 'Executive', 'label': 'Executive Member'},
    {'value': 'General', 'label': 'General Member'},
    {'value': 'Associate', 'label': 'Associate Member'},
    {'value': 'Honorary', 'label': 'Honorary Member'},
    {'value': 'Advisory', 'label': 'Advisory Member'},
  ];

  static const List<Map<String, String>> bloodGroupOptions = [
    {'value': 'APositive', 'label': 'A+'},
    {'value': 'ANegative', 'label': 'A-'},
    {'value': 'BPositive', 'label': 'B+'},
    {'value': 'BNegative', 'label': 'B-'},
    {'value': 'OPositive', 'label': 'O+'},
    {'value': 'ONegative', 'label': 'O-'},
    {'value': 'ABPositive', 'label': 'AB+'},
    {'value': 'ABNegative', 'label': 'AB-'},
  ];
}
