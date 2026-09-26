import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/api_config.dart';
import '../models/auth_model.dart';

class AuthService {
  static const String _tokenKey = 'jwt_token';
  static const String _userKey = 'auth_user_json';

  Future<LoginResponse?> login({
    required String email,
    required String password,
    bool isAdmin = false,
  }) async {
    final url = isAdmin ? ApiConfig.adminLoginUrl : ApiConfig.loginUrl;

    try {
      final response = await http
          .post(
            Uri.parse(url),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'email': email,
              'password': password,
            }),
          )
          .timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final loginRes = LoginResponse.fromJson(data);
        await saveSession(loginRes.token, loginRes.user);
        return loginRes;
      } else {
        final err = jsonDecode(response.body);
        throw Exception(err['message'] ?? 'Login failed (${response.statusCode})');
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<LoginResponse?> devLogin(int adminUserId) async {
    try {
      final response = await http
          .post(
            Uri.parse(ApiConfig.devLoginUrl),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode(adminUserId),
          )
          .timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final loginRes = LoginResponse.fromJson(data);
        await saveSession(loginRes.token, loginRes.user);
        return loginRes;
      } else {
        final err = jsonDecode(response.body);
        throw Exception(err['message'] ?? 'Dev login failed');
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> saveSession(String token, AuthUser user) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
    await prefs.setString(_userKey, jsonEncode(user.toJson()));
  }

  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  Future<AuthUser?> getStoredUser() async {
    final prefs = await SharedPreferences.getInstance();
    final jsonStr = prefs.getString(_userKey);
    if (jsonStr != null && jsonStr.isNotEmpty) {
      try {
        final map = jsonDecode(jsonStr);
        return AuthUser.fromJson(map);
      } catch (_) {
        return null;
      }
    }
    return null;
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
    await prefs.remove(_userKey);
  }
}
