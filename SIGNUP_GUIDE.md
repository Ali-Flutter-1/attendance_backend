# Enhanced Signup API Guide

The signup API now accepts full user profile information including name, email, domain, address, and profile picture.

## 📝 Signup API

### Endpoint
```
POST /api/auth/signup
```

### Content-Type
```
multipart/form-data
```

### Form Fields

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| `firstName` | string | Yes | User's first name |
| `lastName` | string | Yes | User's last name |
| `email` | string | Yes | Valid email address |
| `password` | string | Yes | Minimum 6 characters |
| `confirmPassword` | string | Yes | Must match password |
| `domain` | string | No | Domain/Department |
| `address` | string | No | User's address |
| `picture` | file | No | Profile picture (JPG, PNG, GIF) |

---

## 📱 Flutter Example

### Complete Signup Screen

```dart
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class SignupScreen extends StatefulWidget {
  const SignupScreen({Key? key}) : super(key: key);

  @override
  State<SignupScreen> createState() => _SignupScreenState();
}

class _SignupScreenState extends State<SignupScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _domainController = TextEditingController();
  final _addressController = TextEditingController();
  
  File? _profilePicture;
  bool _isLoading = false;
  String _statusMessage = '';
  final ImagePicker _picker = ImagePicker();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Sign Up')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            children: [
              // Profile Picture
              GestureDetector(
                onTap: _pickImage,
                child: Stack(
                  children: [
                    CircleAvatar(
                      radius: 50,
                      backgroundColor: Colors.grey[300],
                      backgroundImage: _profilePicture != null
                          ? FileImage(_profilePicture!)
                          : null,
                      child: _profilePicture == null
                          ? const Icon(Icons.person, size: 50, color: Colors.grey)
                          : null,
                    ),
                    Positioned(
                      bottom: 0,
                      right: 0,
                      child: Container(
                        padding: const EdgeInsets.all(4),
                        decoration: const BoxDecoration(
                          color: Colors.blue,
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(Icons.camera_alt, color: Colors.white, size: 20),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 8),
              const Text('Tap to add profile picture', style: TextStyle(color: Colors.grey, fontSize: 12)),
              
              const SizedBox(height: 24),
              
              // First Name
              TextFormField(
                controller: _firstNameController,
                decoration: const InputDecoration(
                  labelText: 'First Name *',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please enter your first name';
                  }
                  return null;
                },
              ),
              
              const SizedBox(height: 16),
              
              // Last Name
              TextFormField(
                controller: _lastNameController,
                decoration: const InputDecoration(
                  labelText: 'Last Name *',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please enter your last name';
                  }
                  return null;
                },
              ),
              
              const SizedBox(height: 16),
              
              // Email
              TextFormField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email *',
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
              
              // Password
              TextFormField(
                controller: _passwordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Password *',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please enter a password';
                  }
                  if (value.length < 6) {
                    return 'Password must be at least 6 characters';
                  }
                  return null;
                },
              ),
              
              const SizedBox(height: 16),
              
              // Confirm Password
              TextFormField(
                controller: _confirmPasswordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Confirm Password *',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please confirm your password';
                  }
                  if (value != _passwordController.text) {
                    return 'Passwords do not match';
                  }
                  return null;
                },
              ),
              
              const SizedBox(height: 16),
              
              // Domain
              TextFormField(
                controller: _domainController,
                decoration: const InputDecoration(
                  labelText: 'Domain/Department',
                  border: OutlineInputBorder(),
                ),
              ),
              
              const SizedBox(height: 16),
              
              // Address
              TextFormField(
                controller: _addressController,
                maxLines: 2,
                decoration: const InputDecoration(
                  labelText: 'Address',
                  border: OutlineInputBorder(),
                ),
              ),
              
              const SizedBox(height: 24),
              
              // Status Message
              if (_statusMessage.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.all(8.0),
                  child: Text(
                    _statusMessage,
                    style: TextStyle(
                      color: _statusMessage.contains('success') 
                          ? Colors.green 
                          : Colors.red,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ),
              
              // Signup Button
              ElevatedButton(
                onPressed: _isLoading ? null : _signup,
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 48, vertical: 16),
                  minimumSize: const Size(double.infinity, 50),
                ),
                child: _isLoading
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Sign Up', style: TextStyle(fontSize: 16)),
              ),
              
              const SizedBox(height: 16),
              
              TextButton(
                onPressed: () {
                  // Navigate to login screen
                },
                child: const Text('Already have an account? Login'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  // Pick image from gallery or camera
  Future<void> _pickImage() async {
    try {
      final XFile? image = await _picker.pickImage(
        source: ImageSource.gallery,  // Change to ImageSource.camera for camera
        imageQuality: 85,
        maxWidth: 1024,
        maxHeight: 1024,
      );

      if (image != null) {
        setState(() {
          _profilePicture = File(image.path);
        });
      }
    } catch (e) {
      setState(() {
        _statusMessage = 'Error picking image: $e';
      });
    }
  }

  // Signup
  Future<void> _signup() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    try {
      setState(() {
        _isLoading = true;
        _statusMessage = 'Creating account...';
      });

      // Create multipart request
      var request = http.MultipartRequest(
        'POST',
        Uri.parse('https://localhost:7225/api/auth/signup'), // Replace with your API URL
      );

      // Add form fields
      request.fields['firstName'] = _firstNameController.text;
      request.fields['lastName'] = _lastNameController.text;
      request.fields['email'] = _emailController.text;
      request.fields['password'] = _passwordController.text;
      request.fields['confirmPassword'] = _confirmPasswordController.text;
      
      if (_domainController.text.isNotEmpty) {
        request.fields['domain'] = _domainController.text;
      }
      
      if (_addressController.text.isNotEmpty) {
        request.fields['address'] = _addressController.text;
      }

      // Add profile picture if selected
      if (_profilePicture != null) {
        request.files.add(
          await http.MultipartFile.fromPath(
            'picture',  // ← Important: Field name must be "picture"
            _profilePicture!.path,
          ),
        );
      }

      // Send request
      var streamedResponse = await request.send();
      var response = await http.Response.fromStream(streamedResponse);

      // Parse response
      var responseData = json.decode(response.body);

      setState(() {
        _isLoading = false;
        if (responseData['success'] == true) {
          _statusMessage = 'Account created successfully! ✅';
          // Save user data and navigate to home screen
          final userData = responseData['data'];
          // await saveUserId(userData['id']);
          // Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => HomeScreen()));
        } else {
          _statusMessage = 'Error: ${responseData['message']}';
        }
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _statusMessage = 'Error: $e';
      });
    }
  }

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _domainController.dispose();
    _addressController.dispose();
    super.dispose();
  }
}
```

---

## 🌐 Using cURL

### Signup with All Fields

```bash
curl -X POST "https://localhost:7225/api/auth/signup" \
  -F "firstName=John" \
  -F "lastName=Doe" \
  -F "email=john.doe@example.com" \
  -F "password=password123" \
  -F "confirmPassword=password123" \
  -F "domain=IT" \
  -F "address=123 Main Street" \
  -F "picture=@/path/to/profile.jpg"
```

### Signup without Profile Picture

```bash
curl -X POST "https://localhost:7225/api/auth/signup" \
  -F "firstName=John" \
  -F "lastName=Doe" \
  -F "email=john.doe@example.com" \
  -F "password=password123" \
  -F "confirmPassword=password123"
```

---

## 📋 Postman Example

1. **Method:** `POST`
2. **URL:** `https://localhost:7225/api/auth/signup`
3. **Body:** Select `form-data`
4. **Add fields:**
   - `firstName` (Text): `John`
   - `lastName` (Text): `Doe`
   - `email` (Text): `john@example.com`
   - `password` (Text): `password123`
   - `confirmPassword` (Text): `password123`
   - `domain` (Text): `IT` (optional)
   - `address` (Text): `123 Main St` (optional)
   - `picture` (File): Select image file (optional)

---

## ✅ Success Response

```json
{
  "success": true,
  "message": "Account created successfully",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "domain": "IT",
    "address": "123 Main Street",
    "profilePicturePath": "uploads/profile-pictures/1/abc123.jpg",
    "isAdmin": false
  }
}
```

---

## ❌ Error Responses

### Email Already Exists
```json
{
  "success": false,
  "message": "Email already exists. Please use a different email or login."
}
```

### Passwords Don't Match
```json
{
  "success": false,
  "message": "Password and confirm password do not match"
}
```

### Password Too Short
```json
{
  "success": false,
  "message": "Password must be at least 6 characters long"
}
```

---

## 🔄 Update Profile

After signup, users can update their profile using:

**PUT** `/api/profile/update`

This endpoint accepts the same fields (firstName, lastName, email, domain, address, picture) and allows partial updates.

See `PROFILE_PICTURE_GUIDE.md` for details.

---

## 📝 Notes

1. **Required Fields:** firstName, lastName, email, password, confirmPassword
2. **Optional Fields:** domain, address, picture
3. **Password:** Minimum 6 characters
4. **Profile Picture:** Field name must be `"picture"` (lowercase)
5. **File Types:** JPG, JPEG, PNG, GIF
6. **File Size:** Default max 30MB

---

## 🎯 Complete Flow

1. **User signs up** with all profile information
2. **Account created** → Returns user data with `userId`
3. **User can login** with email and password
4. **User can update profile** later using `/api/profile/update`

The signup API now captures all user information in one step!

