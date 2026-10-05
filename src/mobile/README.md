# 📱 ChannelCenter Mobile Application

> **Flutter Mobile Client** for Doctor Channeling, Consultation Queue & AI Triage Workflows.

---

## 📌 Overview

The **ChannelCenter Mobile App** is a cross-platform mobile application built using **Flutter and Dart**. It serves as the primary mobile interface for patients to search doctors, view available consultation slots, manage appointment tickets, and submit emergency triage intakes for automated safety evaluation.

---

## ✨ Features

- 🔐 **User Authentication**: Secure Login & Registration with JWT token storage via `SharedPreferences`.
- 🩺 **Doctor Directory & Booking**: Browse specialties, inspect doctor schedules, and book channel slots.
- 🚨 **Smart Emergency Triage**: Symptom entry wizard with real-time urgency tagging (Emergency, Urgent, Routine).
- 🎫 **Appointment Management**: Digital ticket viewing, status tracking, and cancellation controls.
- 🔔 **Workflow & State Management**: Clean state management powered by `provider`.

---

## 🛠 Tech Stack

- **Framework**: Flutter 3.x (Dart SDK `>=3.0.0 <4.0.0`)
- **State Management**: `provider`
- **HTTP Client**: `http` package
- **Local Storage**: `shared_preferences`
- **Formatting**: `intl`

---

## 🚀 Local Development Setup

### Prerequisites
- [Flutter SDK](https://docs.flutter.dev/get-started/install) installed and added to your `PATH`.
- Android Studio / VS Code with Flutter extension.
- Running instance of the ASP.NET Core Backend (`http://localhost:5000` or local IP for device emulator).

### Running the App

```bash
# 1. Navigate to mobile directory
cd src/mobile

# 2. Install dependencies
flutter pub get

# 3. Launch application on emulator or connected device
flutter run
```

---

## 🧪 Running Tests

```bash
flutter test
```
