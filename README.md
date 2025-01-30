<img align="left" width="64" height="64" src="https://github.com/coldridj/espresso/raw/develop/src/espresso/Resources/EspressoIcon.ico" alt="espresso icon">

# espresso 

A shot of caffeine for your system.

This periodically sends a key event to the Windows API to prevent idle detection and screen timeout.

An alternate keycode can be sent when a window belonging to an excluded process is in focus,
these are configured in appsettings.json

```json
    "decaffinatedProcessNames": [
        "WindowsTerminal",
        "devenv",
        "OpenConsole"
    ]
```
