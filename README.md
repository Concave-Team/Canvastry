# Canvastry

[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/Concave-Team/Canvastry/blob/main/LICENSE)
[![Open Source](https://badges.frapsoft.com/os/v1/open-source.svg?v=103)](https://opensource.org/)

Canvastry is an open-source general-purpose 2D game engine. It is written in C# and uses Raylib for graphics, and ImGui.NET(+rlImGui) for the editor GUI.
Canvastry uses Lua as it's scripting language, making game development very simple, fast and efficient!

# Features

While Canvastry is still <b>work-in-progress</b> and not fully released yet, current features include:

* Editor Application
* Entities and Components
* Audio Support(using CSCore)
* Graphics (using Raylib)
* Lua Scripting(using MoonSharp)
* Editor Live Preview
* Box Colliders
* Input Processing
* Asset Management
* Events
* Scenes
* Scene Files(and loading/saving them)
* Editor Projects

However, we plan to add more features, like procedural generation, in the future!

# How to Build

Since Canvastry is not fully released yet, we haven't put out any binaries yet, so to use it you need to build it yourself from source.
Do not fret, though! It's really simple, and can be done really quickly.

<b> Using Git + dotnet CLI(or Visual Studio) </b>
<br>
This is probably the most preferrable way to compile Canvastry, as it takes the least amount of time.
<br>
Steps:
* <b>Step 1: Clone the Canvastry repository.</b>
<br>
<b> First download the Git CLI for your OS before proceeding, if you do not have it. </b>
Open up your terminal window where you want to clone the Canvastry repository and run this command: <code>git clone https://github.com/Concave-Team/Canvastry.git</code>
Wait for the operation to finish.

* <b>Step 2: Build the Source Code.</b>
<br>
<b>With the dotnet CLI</b>
Enter the main directory(that contains the solution(.sln) file)
Open your terminal there, and simply run this command: <code>dotnet build Canvastry.Editor -c Release</code>

<b>Using Visual Studio</b>
This is really simple, open up the solution file then check if Canvastry.Editor is the startup project(if it isn't, then set it as that.)
Set the configuration to Release, and then simply just Build.

These should build the Canvastry editor project. <b>If you get any errors</b>, please don't hesitate to create an issue for it!

* <b>Step 3: Open the Executable File.</b>
<br>
Now, go into the Canvastry.Editor folder, and look for the 'bin' folder, in it, go to Release, net8.0 and open up Canvastry.Editor.exe

After doing those steps, you may now use Canvastry to create your games!

# Acknowledgements

Canvastry uses:

* [CSCore](https://github.com/filoe/cscore)
* [Raylib-cs](https://github.com/ChrisDill/Raylib-cs/tree/master)
* [MoonSharp](https://github.com/moonsharp-devs/moonsharp)
* [rlImgui-cs](https://github.com/raylib-extras/rlImGui-cs/tree/main)
* [ImGui.NET](https://github.com/mellinoe/ImGui.NET)
* Newtonsoft.JSON

# License

Canvastry is released under the MIT License, so you are free to use it for commercial uses without having to disclose usage of the engine(although we always appreciate it!)
But if you make changes to the source code, or fix some bugs, we'd appreciate it if you would create a pull request for it! (Though you don't have to.)

# Can I contribute?

Yes! We welcome contributors to help with Canvastry and make it better! You just have to fork this repository and make a pull request once you're done.
Of course, we would like the code to still be readable, so please make sure your code is linted and clean before submitting!
We try to review pull request as fast as we can, so please have some patience!

#

All Rights Reserved, © Concave Studios, 2024.