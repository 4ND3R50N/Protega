# Protega (Anti-Hack Software) 1.0 features -  for Cabal Online
This is a c++ dll which prevents other programms to manipulate the target process. A heartbeat system grants control and many other features for all connected clients. The dll can be called and used in source code, but even if you dont have the code of the target process, we provide a tutorial how to hook the dll in an executable file!

# Features:
## Client
###  Basic Information:
    C++ DLL which is running as a thread in the target application (Has to be included via code or hooked)
    
###  Self Protection:
    - The achitecture (C++, DLL)
    - The hack-detection algorithms are running as threads. Each thread watches the others. If an attacker tries to suspend them, the application closes
    - A watchdog dll also check the thread of the antihack dll. 
    - Heartbeat System (Client to server). The server kicks the user, if the client stops pinging.
    
### Hack Detection - Heuristic Scans:
      - Blacklisted process names
      - Blacklisted MD5 Hashes
### Hack Detection - File Protection
      - File Scanner (Checks MD5 values of specific files)
      - DLL Injection detection (Checks the specific main application)
### Hack Detection - Virtual Memory Protection:
      - Checks the memory of the target protection. If there are irregular changes, it can be detected.
      
      Currently the following hacks can be detected:
      - NSD
      - NCT
      - Range Hack
      - Zoom Hack
      - Speed hack
      - Wallhack
      - Nation Hack
      - No Skill cooldown
      Other sorts of hacks follow soon...
 ## Server
 
 ## Network
 - TCP Based
 - AES-128 Encrypted network stream

# Future Updates - 1.X:
## Client
### Heuristic Scans:
- Blacklisted class names

### Hack Detection - Virtual Memory Protection:
Fixes for the following hacks:
- No stun
- No BM/Aura cooldown
- Kill gate
- No entry

## Server
