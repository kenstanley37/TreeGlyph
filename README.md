# 🌳 TreeGlyph

TreeGlyph is a cross-platform tool that generates clean, readable ASCII trees of folder structures. It supports `.gitignore`-style exclusion patterns, making it ideal for developers, sysadmins, and anyone who wants to visualize directory layouts without the clutter.

Available as both a **Console app** and a **.NET MAUI GUI**, TreeGlyph adapts to your workflow — whether you're scripting on a server or browsing visually on desktop.

---

## 🚀 Features

- 📁 Visualize folder hierarchies in ASCII format
- 🧹 Exclude files/folders using `.gitignore`-style patterns
- 🌐 Global and local ignore rules
- ✂️ Depth control for nested trees
- 📋 Copy output to clipboard
- 💾 Save output to file
- 🖥️ Dual-mode: Console + MAUI GUI

---

## 🧪 Console Usage

```bash
treeglyph [folder] [excludes] [options]


Example
treeglyph src bin,obj,node_modules --depth 2 --save tree.txt --clipboard


Options
|  |  | 
| --depth [n] |  | 
| --save [path] |  | 
| --clipboard |  | 
| --noglobal |  | 
| --setglobal [r] |  | 
| --editglobal |  | 
| --help |  | 



📁 Ignore Rules
TreeGlyph supports three layers of exclusion:
- Global ignore: Stored in
%APPDATA%/TreeGlyph/ignore-global.txt
Use --setglobal or --editglobal to manage.
- Local ignore: Stored in
.treeglyphignore in the target folder.
- CLI patterns: Passed directly as comma-separated values.
All rules are merged and applied during tree generation.

🧩 Architecture
TreeGlyph is built with modularity in mind:
|  |  | 
| Core.Models | FileSystemEntry | 
| Core.Services | TreeBuilderService | 
| Core.Helpers | GlobMatcherFileSystemEntryExtensions | 
| Console |  | 
| UI |  | 


Tree rendering is handled via recursive traversal and indentation logic, producing output like:
└── src/
    ├── Program.cs
    ├── Models/
    │   └── FileSystemEntry.cs
    └── Services/
        └── TreeBuilderService.cs



🛠 Developer Setup
- Clone the repo:
git clone https://github.com/kenstanley37/TreeGlyph.git
cd TreeGlyph
- Build and run the console app:
dotnet build Console/Console.csproj
dotnet run --project Console/Console.csproj
- Or launch the MAUI UI:
dotnet build UI/UI.csproj
dotnet run --project UI/UI.csproj



🤝 Contributing
We welcome contributions!
See CONTRIBUTING.md for setup instructions, coding guidelines, and submission tips.

📄 License
TreeGlyph is released under the MIT License.
By contributing, you agree your code can be licensed under the same terms.

Happy tree rendering! 🌲
