# PROJECT PROPOSAL & TIMELINE PLANNING
**Course:** Network Programming (Lập trình mạng)  
**Project Code:** UDM_08  
**Project Name:** Multi-Client Desktop Chat Application via TCP Protocol  

---

## I. GENERAL INFORMATION

### 1. Group Members
Below is the identification table of our team members along with their corresponding GitHub accounts for progress tracking.

| STT | Full Name | Student ID | GitHub Account ID | Role |
| :---: | :--- | :---: | :---: | :--- |
| 1 | **Huỳnh Nhật Khánh** | 084206002176 | `semiprofan` | Team Leader / Core Network Dev |
| 2 | [**Nhật Tiến**] | 058206003922 | `tienpdn3922-lgtm` | GUI & UX Designer |
| 3 | [**Tiến Đạt**] | 19h1120046 | `19h1120046-beep` | Database & Test Engineer |

* **Project Repository:** [https://github.com/semiprofan/GR12---LTMang](https://github.com/semiprofan/GR12---LTMang)

### 2. General Requirements Alignment
Our project strictly conforms to the course constraints specified by the lecturer:
* **Application Type:** Native GUI Desktop Application (Fully operational on Windows).
* **Architecture:** Traditional Client - Server topology.
* **Strict Constraints:** **NO** Web Application, **NO** Command-Line Interface (Console Application).
* **Deliverables:** Functional binaries, public code repository with strict commit history, stress-test verification, and a demonstrated video record.

---

## II. PROJECT PROPOSAL

### 1. Abstract & Objective (What we want to do)
Our team proposes to develop a **Desktop-based Real-time Chat Application** operating over a centralized server architecture via the **TCP protocol**. The core objective is to create an instantaneous, lightweight, and reliable messaging platform. The application handles concurrent connections smoothly, ensures data delivery without loss or packet reordering, and features a clean graphical user interface inspired by modern design frameworks.

### 2. Core Feature Specifications (What features to implement)
The system will encompass the following production-ready features:
* **Secure Authentication System:** User registration and login mechanics validated against encrypted server-side records.
* **Real-time Private Messaging (1-1 Chat):** Seamless, multi-threaded instant text communication between any two active clients.
* **Dynamic Presence Status Tracking:** Real-time directory system reflecting live status changes (Online, Offline, Away) of registered peers.
* **Visual & Audio Notifications:** Interactive UI alerts triggered upon receiving inbound message packets while active or minimized.
* **Message History Logging:** Light server-side message archiving allowing clients to synchronize and view past chat history immediately upon authentication.

### 3. Technology Stack Selection (What stack we use)
To implement a robust standalone desktop system, we have finalized the following platform stack:

* **Programming Language:** `C#` (.NET Core / .NET 8.0)
* **Graphical User Interface (GUI):** `WPF` (Windows Presentation Foundation) to ensure a fluid layout, responsive components, and visual styling comparable to modern web design.
* **Networking Engine:** `System.Net.Sockets` using asynchronous primitives (`TcpListener` on Server, `TcpClient` on Client) handled via decoupled background workers.
* **Data Interchange Format:** `JSON` serialized through `System.Text.Json` to structure system command packets (Login requests, Message payloads, Status updates).
* **Database Management System:** `SQLite` — an embedded, zero-configuration database engine running locally on the Server node for ultra-fast, relational transaction logging.

### 4. Final Deliverables (What will be achieved)
Upon the completion of the course, our repository will yield:
1. `Server.exe`: A desktop app providing continuous thread hosting, automated network logging, database lookups, and routing logs.
2. `Client.exe`: An intuitive, sleek, and high-performance messaging client.
3. **Stress-Test Portfolio:** Empirical proof of system limits under high-volume simulated packet loads (placed in the `Extra/` directory).
4. **Demonstration Video:** A public link walking through the full operational lifecycle of the product.

---

## III. MILESTONE PLANNING & TIMELINE (2-WEEK PHASES)

To maintain transparent development velocity and avert any consecutive stagnation, tasks are compartmentalized into 4 strict phases:

### 📅 Project Timeline Gantt-Chart Matrix

| Phase | Duration | Primary Focus | Key Milestones & Deliverables |
| :---: | :---: | :--- | :--- |
| **Phase 1** | Week 1 - 2 | System Architecture & GUI Drafts | • Repository workspace configuration.<br>• JSON communication schema definition.<br>• Static WPF mockups (Login & Chat Dashboard). |
| **Phase 2** | Week 3 - 4 | Server Foundation & Authentication | • Asynchronous multi-threaded TCP listener setup.<br>• SQLite schema deployment.<br>• Functional Client connection handshake & Login logic. |
| **Phase 3** | Week 5 - 6 | Core Messaging & Routing Systems | • Server packet redirection algorithms (Client A $\rightarrow$ Server $\rightarrow$ Client B).<br>• Dynamic online status routing updates.<br>• Asynchronous chat UI refresh mechanisms. |
| **Phase 4** | Week 7 - 8 | Error Handling, Stress Tests & Review | • Network timeout & abrupt disconnection handling.<br>• Stress testing & binary packaging (`.exe`).<br>• Full video recording, documentation compilation inside `/DOCX`. |

---

### Phase 1 Detail: Architecture Design & UI Mockup (Week 1 - Week 2)
* **Objectives:** Establish development branches. Map out packet structures. Draft the user flow. Write the visual layout using WPF markup without underlying socket attachments.
* **Deliverables:** Data formatting documentation and non-functional GUI software screens committed inside `/Code`.

### Phase 2 Detail: Core Server Backend & Authentication (Week 3 - Week 4)
* **Objectives:** Instantiate the asynchronous server loop capable of pooling connection sockets. Set up local tables for credential storage. Wire up the client network module to dispatch login payloads and transition UI state based on server response tokens.
* **Deliverables:** Live, query-responsive Server application logging incoming handshakes.

### Phase 3 Detail: Routing Engine & Fluid Real-time Messaging (Week 5 - Week 6)
* **Objectives:** Implement safe multi-client socket dictionary mapping at the server node to seamlessly route traffic between arbitrary endpoint IDs. Establish background worker loops on the client app to process incoming packets independently, avoiding thread blocks on the rendering layer.
* **Deliverables:** Interactive private messaging capability across independent network machines with automated active list refreshes.

### Phase 4 Detail: Edge-Case Handling, Optimization & Stress Testing (Week 7 - Week 8)
* **Objectives:** Build fallback routines protecting the software against server drops or terminal connection loss. Run automated routines testing socket boundaries under persistent messaging. Clean up code refactoring, output stable binaries, record the validation video, and draft the text documentation within `/DOCX`.
* **Deliverables:** Completed deployment binaries, diagnostic logs inside `/Extra`, report inside `/DOCX`, and project presentation file in `/PPTX`.
