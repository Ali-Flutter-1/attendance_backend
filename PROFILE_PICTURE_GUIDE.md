# Profile Picture Upload Guide

## API Endpoint

**PUT** `/api/profile/update`

**Content-Type:** `multipart/form-data`

## How to Upload Profile Picture

### Endpoint Details
- **URL:** `https://localhost:7225/api/profile/update`
- **Method:** `PUT`
- **Content-Type:** `multipart/form-data`

### Form Fields

| Field Name | Type | Required | Description |
|------------|------|----------|-------------|
| `userId` | int | Yes | User ID |
| `firstName` | string | No | First name |
| `lastName` | string | No | Last name |
| `email` | string | No | Email address |
| `domain` | string | No | Domain/Department |
| `picture` | file | No | Profile picture (JPG, PNG, GIF) |

### Important Notes
- **Picture field name must be `"picture"`** (lowercase)
- All other fields are optional
- You can update just the picture without updating other fields
- Old picture is automatically deleted when a new one is uploaded

---

## 📱 Flutter Example

### Complete Flutter Code for Profile Picture Upload

```dart
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class ProfileUpdateScreen extends StatefulWidget {
  final int userId;
  
  const ProfileUpdateScreen({Key? key, required this.userId}) : super(key: key);

  @override
  State<ProfileUpdateScreen> createState() => _ProfileUpdateScreenState();
}

class _ProfileUpdateScreenState extends State<ProfileUpdateScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _domainController = TextEditingController();
  
  File? _profilePicture;
  bool _isLoading = false;
  String _statusMessage = '';
  final ImagePicker _picker = ImagePicker();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Update Profile'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            children: [
              // Profile Picture Section
              GestureDetector(
                onTap: _pickImage,
                child: Stack(
                  children: [
                    CircleAvatar(
                      radius: 60,
                      backgroundColor: Colors.grey[300],
                      backgroundImage: _profilePicture != null
                          ? FileImage(_profilePicture!)
                          : null,
                      child: _profilePicture == null
                          ? const Icon(Icons.person, size: 60, color: Colors.grey)
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
              const Text(
                'Tap to change profile picture',
                style: TextStyle(color: Colors.grey, fontSize: 12),
              ),
              
              const SizedBox(height: 24),
              
              // First Name
              TextFormField(
                controller: _firstNameController,
                decoration: const InputDecoration(
                  labelText: 'First Name',
                  border: OutlineInputBorder(),
                ),
              ),
              
              const SizedBox(height: 16),
              
              // Last Name
              TextFormField(
                controller: _lastNameController,
                decoration: const InputDecoration(
                  labelText: 'Last Name',
                  border: OutlineInputBorder(),
                ),
              ),
              
              const SizedBox(height: 16),
              
              // Email
              TextFormField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value != null && value.isNotEmpty) {
                    if (!value.contains('@')) {
                      return 'Please enter a valid email';
                    }
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
              
              // Update Button
              ElevatedButton(
                onPressed: _isLoading ? null : _updateProfile,
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
                    : const Text('Update Profile', style: TextStyle(fontSize: 16)),
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

  // Update profile
  Future<void> _updateProfile() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    try {
      setState(() {
        _isLoading = true;
        _statusMessage = 'Updating profile...';
      });

      // Create multipart request
      var request = http.MultipartRequest(
        'PUT',
        Uri.parse('https://localhost:7225/api/profile/update'), // Replace with your API URL
      );

      // Add form fields
      request.fields['userId'] = widget.userId.toString();
      
      if (_firstNameController.text.isNotEmpty) {
        request.fields['firstName'] = _firstNameController.text;
      }
      
      if (_lastNameController.text.isNotEmpty) {
        request.fields['lastName'] = _lastNameController.text;
      }
      
      if (_emailController.text.isNotEmpty) {
        request.fields['email'] = _emailController.text;
      }
      
      if (_domainController.text.isNotEmpty) {
        request.fields['domain'] = _domainController.text;
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
          _statusMessage = 'Profile updated successfully! ✅';
          // Optionally navigate back or refresh profile
        } else {
          _statusMessage = 'Error: ${responseData['message']}';
        }
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _statusMessage = 'Error updating profile: $e';
      });
    }
  }

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _domainController.dispose();
    super.dispose();
  }
}
```

---

## 🌐 Using cURL

### Update Profile with Picture

```bash
curl -X PUT "https://localhost:7225/api/profile/update" \
  -F "userId=1" \
  -F "firstName=John" \
  -F "lastName=Doe" \
  -F "email=john.doe@company.com" \
  -F "domain=IT" \
  -F "picture=@/path/to/profile.jpg"
```

### Update Only Picture (without other fields)

```bash
curl -X PUT "https://localhost:7225/api/profile/update" \
  -F "userId=1" \
  -F "picture=@/path/to/profile.jpg"
```

### Update Only Name (without picture)

```bash
curl -X PUT "https://localhost:7225/api/profile/update" \
  -F "userId=1" \
  -F "firstName=John" \
  -F "lastName=Smith"
```

---

## 📋 Postman Example

### Setup in Postman:

1. **Method:** `PUT`
2. **URL:** `https://localhost:7225/api/profile/update`
3. **Body:** Select `form-data`
4. **Add fields:**
   - `userId` (Text): `1`
   - `firstName` (Text): `John` (optional)
   - `lastName` (Text): `Doe` (optional)
   - `email` (Text): `john@company.com` (optional)
   - `domain` (Text): `IT` (optional)
   - `picture` (File): Select image file

---

## ✅ Response Format

### Success Response

```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@company.com",
    "domain": "IT",
    "address": "123 Main St",
    "profilePicturePath": "uploads/profile-pictures/1/abc123.jpg",
    "isAdmin": false
  }
}
```

### Error Response

```json
{
  "success": false,
  "message": "User not found"
}
```

---

## 📍 Get Profile Picture URL

After uploading, the profile picture path is returned in the response. To access the image:

```
https://localhost:7225/uploads/profile-pictures/{userId}/{filename}
```

Example:
```
https://localhost:7225/uploads/profile-pictures/1/abc123.jpg
```

---

## 🔧 SQL Query to Update Profile Picture Path

If you need to update the profile picture path directly in the database:

```sql
-- Update profile picture path
UPDATE Users
SET 
    ProfilePicturePath = 'uploads/profile-pictures/1/new-picture.jpg',
    UpdatedAt = GETUTCDATE()
WHERE Id = 1;
```

---

## 📝 Important Notes

1. **Field Name:** The picture field **must** be named `"picture"` (lowercase)
2. **File Types:** Supported formats are JPG, JPEG, PNG, GIF
3. **File Size:** Default max size is 30MB (can be configured)
4. **Old Picture:** Automatically deleted when a new one is uploaded
5. **Optional Fields:** All fields except `userId` are optional
6. **Partial Updates:** You can update only the fields you want to change

---

## 🐛 Troubleshooting

### Issue: Picture not uploading

**Solution:**
- Check that field name is exactly `"picture"` (lowercase)
- Verify file size is under 30MB
- Check file format (JPG, PNG, GIF only)
- Ensure `wwwroot/uploads/` folder exists and has write permissions

### Issue: Old picture not deleted

**Solution:**
- Check file path in database
- Verify file exists at the path
- Check file permissions

### Issue: 404 Error when accessing picture

**Solution:**
- Ensure static files are enabled in `Program.cs`
- Check that `wwwroot` folder exists
- Verify the file path in the database matches the actual file location

---

## 🎯 Quick Test

1. **Get current profile:**
   ```http
   GET /api/profile/1
   ```

2. **Update profile with picture:**
   ```http
   PUT /api/profile/update
   Form Data:
   - userId: 1
   - picture: [select image file]
   ```

3. **Verify update:**
   ```http
   GET /api/profile/1
   ```

The `profilePicturePath` should now contain the new image path!

