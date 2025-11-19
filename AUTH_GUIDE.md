# Authentication API Guide

This guide explains how to use the Signup and Login APIs.

## 🔐 Authentication Endpoints

### 1. Signup (Create Account)
**POST** `/api/auth/signup`

### 2. Login
**POST** `/api/auth/login`

---

## 📝 Signup API

### Endpoint
```
POST /api/auth/signup
```

### Request Body
```json
{
  "email": "user@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

### Validation Rules
- **Email**: Required, must be valid email format
- **Password**: Required, minimum 6 characters
- **ConfirmPassword**: Required, must match password

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Account created successfully",
  "data": {
    "id": 1,
    "firstName": "",
    "lastName": "",
    "email": "user@example.com",
    "domain": null,
    "address": null,
    "profilePicturePath": null,
    "isAdmin": false
  }
}
```

### Error Responses

**Email Already Exists (400)**
```json
{
  "success": false,
  "message": "Email already exists. Please use a different email or login."
}
```

**Passwords Don't Match (400)**
```json
{
  "success": false,
  "message": "Password and confirm password do not match"
}
```

**Password Too Short (400)**
```json
{
  "success": false,
  "message": "Password must be at least 6 characters long"
}
```

---

## 🔑 Login API

### Endpoint
```
POST /api/auth/login
```

### Request Body
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "user@example.com",
    "domain": "IT",
    "address": "123 Main St",
    "profilePicturePath": "uploads/profile-pictures/1/pic.jpg",
    "isAdmin": false
  }
}
```

### Error Responses

**Invalid Credentials (401)**
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

---

## 📱 Flutter Examples

### Signup Example

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

Future<Map<String, dynamic>> signup(String email, String password, String confirmPassword) async {
  try {
    final response = await http.post(
      Uri.parse('https://localhost:7225/api/auth/signup'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'email': email,
        'password': password,
        'confirmPassword': confirmPassword,
      }),
    );

    final data = json.decode(response.body);
    
    if (data['success'] == true) {
      // Save user data locally
      final userData = data['data'];
      // Store userId for future API calls
      // await saveUserId(userData['id']);
      return {'success': true, 'user': userData};
    } else {
      return {'success': false, 'message': data['message']};
    }
  } catch (e) {
    return {'success': false, 'message': 'Error: $e'};
  }
}
```

### Login Example

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

Future<Map<String, dynamic>> login(String email, String password) async {
  try {
    final response = await http.post(
      Uri.parse('https://localhost:7225/api/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'email': email,
        'password': password,
      }),
    );

    final data = json.decode(response.body);
    
    if (data['success'] == true) {
      // Save user data locally
      final userData = data['data'];
      // Store userId for future API calls
      // await saveUserId(userData['id']);
      return {'success': true, 'user': userData};
    } else {
      return {'success': false, 'message': data['message']};
    }
  } catch (e) {
    return {'success': false, 'message': 'Error: $e'};
  }
}
```

### Complete Flutter Login Screen Example

```dart
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class LoginScreen extends StatefulWidget {
  const LoginScreen({Key? key}) : super(key: key);

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLoading = false;
  String _errorMessage = '';

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Login')),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              TextFormField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please enter your email';
                  }
                  if (!value.contains('@')) {
                    return 'Please enter a valid email';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _passwordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Password',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please enter your password';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 24),
              if (_errorMessage.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.all(8.0),
                  child: Text(
                    _errorMessage,
                    style: const TextStyle(color: Colors.red),
                  ),
                ),
              ElevatedButton(
                onPressed: _isLoading ? null : _login,
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 48, vertical: 16),
                  minimumSize: const Size(double.infinity, 50),
                ),
                child: _isLoading
                    ? const CircularProgressIndicator()
                    : const Text('Login', style: TextStyle(fontSize: 16)),
              ),
              const SizedBox(height: 16),
              TextButton(
                onPressed: () {
                  // Navigate to signup screen
                },
                child: const Text('Don\'t have an account? Sign up'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = '';
    });

    try {
      final response = await http.post(
        Uri.parse('https://localhost:7225/api/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'email': _emailController.text,
          'password': _passwordController.text,
        }),
      );

      final data = json.decode(response.body);

      if (data['success'] == true) {
        // Save user data
        final userData = data['data'];
        // Navigate to home screen or save user data
        // Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => HomeScreen(userId: userData['id'])));
      } else {
        setState(() {
          _errorMessage = data['message'];
        });
      }
    } catch (e) {
      setState(() {
        _errorMessage = 'Error: $e';
      });
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }
}
```

---

## 🌐 Using cURL

### Signup
```bash
curl -X POST "https://localhost:7225/api/auth/signup" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123",
    "confirmPassword": "password123"
  }'
```

### Login
```bash
curl -X POST "https://localhost:7225/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

---

## 📋 Postman Examples

### Signup Request
1. **Method:** `POST`
2. **URL:** `https://localhost:7225/api/auth/signup`
3. **Headers:**
   - `Content-Type: application/json`
4. **Body (raw JSON):**
   ```json
   {
     "email": "user@example.com",
     "password": "password123",
     "confirmPassword": "password123"
   }
   ```

### Login Request
1. **Method:** `POST`
2. **URL:** `https://localhost:7225/api/auth/login`
3. **Headers:**
   - `Content-Type: application/json`
4. **Body (raw JSON):**
   ```json
   {
     "email": "user@example.com",
     "password": "password123"
   }
   ```

---

## 🔒 Password Security

- Passwords are hashed using **SHA256** algorithm
- Passwords are never stored in plain text
- Passwords are never returned in API responses
- Minimum password length: 6 characters

---

## 📊 SQL Queries

### Create User with Password (Direct SQL)
```sql
-- Note: You need to hash the password first using SHA256
-- This is just for reference - use the API instead

-- Example: Password "password123" hashed
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt)
VALUES (
    'user@example.com',
    'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F', -- Hashed password
    'John',
    'Doe',
    GETUTCDATE()
);
```

### Login Verification (Direct SQL)
```sql
-- This is just for reference - use the API instead
-- You would need to hash the input password and compare

SELECT Id, FirstName, LastName, Email, Domain, Address, ProfilePicturePath, IsAdmin
FROM Users
WHERE Email = 'user@example.com'
  AND PasswordHash = 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F';
```

---

## ⚠️ Important Notes

1. **No JWT Tokens**: This implementation does NOT use JWT tokens. After login, store the `userId` from the response and use it for subsequent API calls.

2. **Password Storage**: Passwords are hashed using SHA256. For production, consider using bcrypt or Argon2 for better security.

3. **Session Management**: Since there's no JWT, you'll need to:
   - Store `userId` in your Flutter app (SharedPreferences, SecureStorage, etc.)
   - Send `userId` with each API request that requires authentication
   - Handle logout by clearing the stored `userId`

4. **Security Recommendations**:
   - Use HTTPS in production
   - Consider implementing rate limiting
   - Add password strength requirements
   - Consider implementing password reset functionality

---

## 🚀 Quick Test

1. **Signup:**
   ```bash
   POST /api/auth/signup
   {
     "email": "test@example.com",
     "password": "test123",
     "confirmPassword": "test123"
   }
   ```

2. **Login:**
   ```bash
   POST /api/auth/login
   {
     "email": "test@example.com",
     "password": "test123"
   }
   ```

3. **Use userId from login response for other API calls:**
   ```bash
   GET /api/profile/{userId}
   POST /api/attendance/checkin
   Form Data: userId={userId from login}
   ```

---

## 🔄 Migration Required

After adding password field, run:

```bash
dotnet ef migrations add AddPasswordToUser
dotnet ef database update
```

---

## 📝 Example Flow

1. **User signs up** → Gets `userId` in response
2. **User logs in** → Gets `userId` and user data in response
3. **Store `userId`** in Flutter app (SharedPreferences/SecureStorage)
4. **Use `userId`** for all subsequent API calls:
   - Check-in: `POST /api/attendance/checkin` with `userId` in form data
   - Get profile: `GET /api/profile/{userId}`
   - Apply leave: `POST /api/leave/apply` with `userId` in body
   - etc.

---

The authentication system is now ready! Users can signup and login with email and password.

