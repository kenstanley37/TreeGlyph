🤝 Contributing to TreeGlyph
Thanks for your interest in contributing to TreeGlyph! We welcome ideas, bug fixes, and enhancements that improve the experience for developers and everyday users alike.

🛠 Getting Started
- Fork and clone the repository:
git clone https://github.com/kenstanley37/TreeGlyph.git
cd TreeGlyph
- Open the solution in Visual Studio 2022 or later
Make sure the following workloads are installed:
- ✅ .NET MAUI
- ✅ Windows App SDK
- Build and run the app:
- For the MAUI UI:
dotnet build UI/UI.csproj
dotnet run --project UI/UI.csproj
- For the Console app:
dotnet build Console/Console.csproj
dotnet run --project Console/Console.csproj



🌿 Branching Strategy
- main: Stable release branch. Only updated via tested merges from dev.
- dev: Active development branch. All feature branches merge here.
- feature/*: Create a new branch for each feature or fix.
Example:
git checkout -b feature/improve-ignore-ui


Submit pull requests to dev, not main. We merge dev into main for releases.

🧪 Submitting Changes
- Create a feature branch:
git checkout -b feature/my-contribution
- Make your changes with clear, atomic commits.
- Test your changes thoroughly in both UI and Console modes (if applicable).
- Submit a Pull Request with:
- A descriptive title
- A short explanation of your fix or feature
- Screenshots or previews if it's UI-related

💡 Code Guidelines
- Follow existing code structure and formatting conventions.
- ViewModels use MVVM via CommunityToolkit.Mvvm.
- UI elements favor desktop-style responsiveness and minimalism.
- Avoid code duplication — shared logic belongs in Core/.
- Use .gitignore-style patterns consistently for filtering logic.

📁 Project Layout
|  |  | 
| Core/ |  | 
| UI/ |  | 
| Console/ |  | 
| assets/ |  | 
| temp/ |  | 



📄 License
TreeGlyph is released under the MIT License.
By contributing, you agree your code can be licensed under the same terms.

Happy tree rendering! 🌲
Let’s build something beautiful and maintainable — together.

