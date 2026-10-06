# Screens map

```mermaid
stateDiagram-v2
    [*] --> AccountSelect
    AccountSelect --> MainPage
    AccountSelect --> LoginScreen
    AccountSelect --> RegisterScreen
    LoginScreen --> MainPage
    RegisterScreen --> MainPage
    MainPage --> [*]
```
