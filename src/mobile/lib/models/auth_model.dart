class AuthUser {
  final int id;
  final String fullName;
  final String email;
  final String role; // "Patient", "Doctor", "Staff", "Admin"

  AuthUser({
    required this.id,
    required this.fullName,
    required this.email,
    required this.role,
  });

  factory AuthUser.fromJson(Map<String, dynamic> json) {
    return AuthUser(
      id: json['id'] ?? json['userId'] ?? 0,
      fullName: json['fullName'] ?? json['name'] ?? 'User',
      email: json['email'] ?? '',
      role: json['role']?.toString() ?? json['UserRole']?.toString() ?? 'Patient',
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'fullName': fullName,
    'email': email,
    'role': role,
  };

  bool get isAdmin => role.toLowerCase() == 'admin';
  bool get isDoctor => role.toLowerCase() == 'doctor';
  bool get isPatient => role.toLowerCase() == 'patient';
  bool get isStaff => role.toLowerCase() == 'staff';
}

class LoginResponse {
  final String token;
  final String tokenType;
  final AuthUser user;
  final String? expiresAt;

  LoginResponse({
    required this.token,
    required this.tokenType,
    required this.user,
    this.expiresAt,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    Map<String, dynamic> userData = {};
    if (json.containsKey('user') && json['user'] != null) {
      userData = json['user'];
    } else if (json.containsKey('adminUser') && json['adminUser'] != null) {
      userData = json['adminUser'];
    } else {
      userData = json;
    }

    return LoginResponse(
      token: json['token'] ?? '',
      tokenType: json['tokenType'] ?? 'Bearer',
      user: AuthUser.fromJson(userData),
      expiresAt: json['expiresAt']?.toString(),
    );
  }
}
