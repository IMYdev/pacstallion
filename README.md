<p align="center"><b>PacStallion</b></p>

<p align="center"><b>The GUI for <a href="https://pacstall.dev/">Ubuntu's AUR</a></b></p>

---
## Features

- Explore packages available in the Pacstall repositories.
- Easily install or remove packages.
- See which packages need to be updated.
- Runs Pacstall commands in the terminal for transparency.
---

## Installation
Grab a [release](https://github.com/IMYdev/pacstallion/releases/) from here or [build yourself](#building).

## Building
**[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** is required for developing this application.

#### Clone the repository
```bash
git clone https://github.com/IMYdev/pacstallion.git
cd pacstallion
```

#### Build and Run
To run the application in development mode:
```bash
dotnet run --project src/pacstallion.csproj
```

#### Publish
To create a standalone executable:
```bash
dotnet publish src/pacstallion.csproj -c Release
```
---
## Project Structure

- `src/`: Contains the source code and assets.
- `LICENSE`: GPLv3 License.
- `README.md`: Project documentation.

## License
---
![GPLv3](https://www.gnu.org/graphics/gplv3-with-text-136x68.png)
```monospace
PacStallion is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License

PacStallion is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with PacStallion. If not, see <https://www.gnu.org/licenses/>.
```