# PROJECT PROPOSAL & TIMELINE PLANNING
**Course:** Network Programming (Lập trình mạng)  
**Project Code:** UDM_08  
**Project Name:** Multi-Client Desktop Chat Application via TCP Protocol  

---

## I. GENERAL INFORMATION

### 1. Group Members
Below is the identification table of our team members along with their corresponding GitHub accounts for progress tracking.

| STT | Full Name | Student ID | GitHub Account ID | Role & Responsibilities |
| :---: | :--- | :---: | :--- | :--- |
| 1 | HUỲNH NHẬT KHÁNH | 084206002176 | `semiprofan` | Team Leader / Server Backend Dev |
| 2 | PHAN ĐIỀN NHẬT TIẾN | 058206003922 | `[tienpdn3922-lgtm]` | Client UI/UX Designer |
| 3 | ĐÀO TIẾN ĐẠT | 19h1120046 | `[19h1120046-beep]` | Client Network Logic Dev |
| 4 | BÙI THANH HÒA | `051206011215` | `Thanhhoa066` | Database & History Dev |
| 5 | NGUYỄN MINH PHÚ | `084205002407` | `Phunguyen4i4o` | QA, Tester & Documentation |

* **Project Repository:** [https://github.com/semiprofan/GR12---LTMang](https://github.com/semiprofan/GR12---LTMang)

### 2. General Requirements Alignment
Our project strictly conforms to the course constraints specified by the lecturer:
* **Application Type:** Native GUI Desktop Application (Fully operational on Windows).
* **Architecture:** Traditional Client - Server topology.
* **Strict Constraints:** **NO** Web Application, **NO** Command-Line Interface (Console Application).
* **Deliverables:** Functional binaries, public code repository with strict commit history, stress-test verification, and a demonstrated video record.

---

## II. PROJECT PROPOSAL

### 1. Abstract & Objective
Our team proposes to develop a **Desktop-based Real-time Chat Application** operating over a centralized server architecture via the **TCP protocol**. The core objective is to create an instantaneous, lightweight, and reliable messaging platform.

### 2. Core Feature Specifications
* **Secure Authentication:** User registration and login mechanics.
* **Real-time Private Messaging:** 1-to-1 instant text communication between clients.
* **Presence Status Tracking:** Live status changes (Online/Offline) of registered peers.
* **Visual Notifications:** UI alerts triggered upon receiving inbound message packets.
* **Message History Logging:** Server-side message archiving allowing clients to view past chats.

### 3. Technology Stack Selection
* **Programming Language:** `C#` (.NET)
* **GUI Framework:** `WPF` or `Windows Forms`
* **Networking Engine:** `System.Net.Sockets` (`TcpListener` / `TcpClient`)
* **Data Format:** `JSON` (`System.Text.Json`)
* **Database:** `SQLite` (Embedded server-side database)

### 4. Final Deliverables
1. `Server.exe` and `Client.exe` binaries.
2. **Stress-Test Portfolio** (placed in the `/Extra` directory).
3. **Demonstration Video** & **Project Report** (inside `/DOCX`).

---

## III. MILESTONE PLANNING & TIMELINE (2-WEEK PHASES)

| Phase | Duration | Primary Focus | Key Milestones & Deliverables |
| :---: | :---: | :--- | :--- |
| **Phase 1** | Week 1 - 2 | System Architecture & GUI Drafts | • Repository configuration.<br>• JSON schema definition.<br>• Static GUI mockups (Login & Chat). |
| **Phase 2** | Week 3 - 4 | Server Foundation & Authentication | • Asynchronous TCP listener setup.<br>• SQLite deployment.<br>• Client connection & Login logic. |
| **Phase 3** | Week 5 - 6 | Core Messaging & Routing Systems | • Server packet redirection algorithms.<br>• Online status routing updates.<br>• Asynchronous chat UI refresh. |
| **Phase 4** | Week 7 - 8 | Error Handling, Stress Tests & Review | • Network timeout handling.<br>• Stress testing & `.exe` packaging.<br>• Final video recording & docs (`/DOCX`). |
