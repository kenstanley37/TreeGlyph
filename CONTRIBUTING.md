# 🤝 Contributing to TreeGlyph

Thanks for your interest in contributing to TreeGlyph! We welcome ideas, bug fixes, and enhancements that improve the experience for developers and everyday users alike.

---

## 🛠 Getting Started

1. **Fork and clone the repository**:
   ```bash
   git clone https://github.com/kenstanley37/TreeGlyph.git
   cd TreeGlyph


- Open the solution in Visual Studio 2022 or later.
Make sure the following workloads are installed:
- .NET MAUI
- Windows App SDK
- Build and run the app:
- For the MAUI UI:
dotnet build UI/UI.csproj
dotnet run --project UI/UI.csproj
- For the Console app:
dotnet build Console/Console.csproj
dotnet run --project Console/Console.csproj
🧪 Submitting Changes- Create a feature branch:
git checkout -b feature/my-contribution
- Make your changes with clear, atomic commits.
- Test your changes thoroughly in both UI and Console modes (if applicable).
- Submit a Pull Request with:
- A descriptive title
- A short explanation of your fix or feature
- Screenshots or previews if it's UI-related
💡 Code Guidelines- Follow existing code structure and formatting conventions.
- ViewModels use MVVM via CommunityToolkit.Mvvm.
- UI elements favor desktop-style responsiveness and minimalism.
- Avoid code duplication — shared logic belongs in Core.
- Use .gitignore-style patterns consistently for filtering logic.
📁 Project Layout- Core/ — Shared logic and services
- UI/ — .NET MAUI app for graphical users
- Console/ — CLI app for automation and server use
- assets/ — Visuals and branding
- temp/ — Build and publish logs
📄 LicenseTreeGlyph is released under the MIT License.
By contributing, you agree your code can be licensed under the same terms.Happy tree rendering! 🌲