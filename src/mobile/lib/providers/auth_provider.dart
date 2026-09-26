import 'package:flutter/foundation.dart';
import '../models/auth_model.dart';
import '../services/auth_service.dart';

class AuthProvider with ChangeNotifier {
  final AuthService _authService = AuthService();

  bool _isLoading = true;
  bool get isLoading => _isLoading;

  String? _errorMessage;
  String? get errorMessage => _errorMessage;

  String? _token;
  String? get token => _token;

  AuthUser? _currentUser;
  AuthUser? get currentUser => _currentUser;

  bool get isAuthenticated => _token != null && _token!.isNotEmpty && _currentUser != null;
  bool get isAdmin => _currentUser?.isAdmin ?? false;
  bool get isDoctor => _currentUser?.isDoctor ?? false;
  bool get isPatient => _currentUser?.isPatient ?? true;
  bool get isStaff => _currentUser?.isStaff ?? false;

  AuthProvider() {
    _initAuth();
  }

  Future<void> _initAuth() async {
    _isLoading = true;
    notifyListeners();

    try {
      _token = await _authService.getToken();
      _currentUser = await _authService.getStoredUser();
    } catch (e) {
      _token = null;
      _currentUser = null;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> login({
    required String email,
    required String password,
    bool isAdmin = false,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final response = await _authService.login(
        email: email,
        password: password,
        isAdmin: isAdmin,
      );

      if (response != null) {
        _token = response.token;
        _currentUser = response.user;
        _isLoading = false;
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<bool> devLogin(int adminUserId) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final response = await _authService.devLogin(adminUserId);
      if (response != null) {
        _token = response.token;
        _currentUser = response.user;
        _isLoading = false;
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<void> logout() async {
    await _authService.logout();
    _token = null;
    _currentUser = null;
    notifyListeners();
  }

  void clearError() {
    _errorMessage = null;
    notifyListeners();
  }
}
