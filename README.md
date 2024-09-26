# Password Vault Extension

## Final Report for the Graduation Project in Secure Code Execution Environments
# Password Wallet Chrome Extension

### Authors:
- **Rivki Shachar**  
  Computer Science, Year 3, Semester 1  
  Academic Center Lev, Tivona Campus  

- **Atara Ginzburg**  
  Computer Science, Year 4, Semester 1  
  Academic Center Lev, Tivona Campus  

### Supervisor: 
- Dr. Barak Einav

---

## Table of Contents
1. [Problem Definition](#problem-definition)
2. [Solution Overview](#solution-overview)
3. [Project Description](#project-description)
4. [Advantages Over Other Solutions](#advantages-over-other-solutions)
5. [Security Analysis](#security-analysis)
6. [Future Improvements](#future-improvements)
7. [Work Distribution](#work-distribution)
8. [Installation Guide](#installation-guide)
9. [Server Execution Guide](#server-execution-guide)
10. [Using the Extension](#using-the-extension)

---

## Problem Definition
Many websites require user registration and login, typically through a username and password. Personal information, such as medical records and credit card details, are often stored on these sites. The challenge arises from the difficulty of remembering multiple passwords. Users often resort to using a single password across sites, which can lead to widespread access if one site is compromised. Alternatively, users might create unique passwords for each site but struggle to remember them, resulting in insecure storage practices.

## Solution Overview
Our solution utilizes a secure hardware-based environment that operates parallel to the main operating system (Android, Linux, OS X, Windows). This Trusted Execution Environment (TEE) runs on the computer's main chips, allowing for secure access to system resources. Our approach involves generating random, strong passwords for each site and securely storing them in this environment. This method minimizes exposure to potential breaches, even if the main operating system is compromised.

## Project Description
We developed a Chrome extension that identifies the website the user is visiting and generates secure passwords while allowing for the import of existing passwords. Upon first use, the user sets a master password, which is securely stored in the TEE. When registering on a new site, the user inputs their details, and upon entering the master password, the extension generates a strong username and password. The extension will autofill these credentials when logging into existing sites.

### Memory Structure Example
**Personal Password:** 11112222

| Website URL                     | Password    | Username          |
|----------------------------------|-------------|--------------------|
| https://www.kosher.com          | KQnC=s8v    | example@gmail.com     |
| https://lib.biu.ac.il           | 7E$ivppr    | RivkaShachar        |
| https://shop.super-pharm.co.il  | P_J4auFe    | NAME              |
| https://account.next.co.il      | FdM5vne&    | example@gmail.com     |


## Advantages Over Other Solutions
Existing password vaults use various encryption methods, but they still store passwords on potentially vulnerable systems. Our solution stores passwords in a secure environment that is not accessible to the main operating system, ensuring enhanced security. The user must input their master password to access their stored passwords, preventing unauthorized access even if the system is compromised.

## Security Analysis
The primary goal of security in our project is to ensure that communication with the secure environment occurs through a secure channel. The extension validates input, and if the master password is incorrect, it prevents further actions. The TEE operates independently, ensuring that even if the main operating system is compromised, access to stored passwords remains secure.

## Future Improvements
- Mobile support for password retrieval on phones or other computers.
- Enhanced encryption methods for data transmission.
- Additional user-friendly features to improve accessibility and usability.

## Work Distribution
- Rivki Shachar: Development of the Chrome extension and frontend integration.
- Atara Ginzburg: Development of the secure backend and system architecture.

## Installation Guide
1. Clone or download this repository.
2. Open Chrome and navigate to `chrome://extensions/`.
3. Enable "Developer mode."
4. Click "Load unpacked" and select the extension folder.

## Server Execution Guide
To run the server, ensure you have the required dependencies installed. Use the command below to start the server:

```bash
# Command to start the server
npm start
```

### Using the Extension

1. **Installation**
   - Download and install the extension from the Chrome Web Store.
   - Once installed, the extension icon will appear in your browser's toolbar.

2. **Accessing the Password Vault**
   - Click the extension icon to open the password vault.
   - You will be prompted to enter your master password to access your stored passwords.

3. **Adding a New Password**
   - After logging in, click on the "Add Password" button.
   - Fill in the website URL, username, and password fields.
   - Click "Save" to store your new password.

4. **Viewing Passwords**
   - In the main vault interface, you will see a list of saved passwords.
   - Click on any entry to view details, including the website URL, username, and password.

5. **Editing a Password**
   - To edit an existing password, click on the entry you wish to modify.
   - Make the necessary changes and click "Save" to update the information.

6. **Deleting a Password**
   - To delete a password, click on the entry and select the "Delete" option.
   - Confirm the deletion to remove it from your vault.

7. **Security**
   - Ensure you keep your master password secure, as it protects all your stored passwords.
   - Use the extension on trusted devices only.


Welcome to the Password Wallet Chrome Extension! This tool helps you securely manage your passwords with ease and efficiency.


## Installation

To install the Password Wallet extension, follow the steps outlined in the demo video below.

### Watch the Demo Video
[![Install](https://img.youtube.com/vi/gU13XbYh5-M/0.jpg)](https://youtu.be/gU13XbYh5-M?si=JWief4QRhQWRMe0u)

## Registration

In this demo video, you will see how to register on a website using the Password Wallet extension. The extension will generate a strong, safe password for you automatically.

### Watch the Demo Video
[![Register](https://img.youtube.com/vi/I5K6M6ehFOQ/0.jpg)](https://youtu.be/I5K6M6ehFOQ?si=TS9UoWctjiBJL4Sy)

## Login

This demo video shows how to log into a website using the Password Wallet extension. It will demonstrate the auto-complete feature that fills in your safe password seamlessly.

### Watch the Demo Video
[![Login](https://img.youtube.com/vi/OxLo-WNkX_Q/0.jpg)](https://youtu.be/OxLo-WNkX_Q?si=7ou0r8AnNVh9G9Ou)

Click on the images to play the videos.

