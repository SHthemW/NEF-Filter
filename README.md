# NEF-Filter

## 中文

一个用于清理相机 `NEF` 文件的命令行工具喵。

它会根据 `JPG` 文件是否存在，删除没有对应图片的 `NEF` 文件喵。

### 功能

- 默认仅扫描当前目录喵。
- 传入 `--recursive` 或 `-r` 时递归扫描子目录喵。
- 可以分别指定 `NEF` 目录和 `JPG` 目录喵。
- `JPG` 文件不做任何修改喵。
- 默认会先列出待删除文件并要求二次确认，传入 `--quiet` 或 `-q` 时直接执行喵。

### 用法

```bash
neff [--nef-dir <目录>] [--jpg-dir <目录>] [--quiet] [--recursive]
```

### 参数

- `--nef-dir, -n`：指定 `NEF` 目录，默认当前目录喵。
- `--jpg-dir, -j`：指定 `JPG` 目录，默认当前目录喵。
- `--quiet, -q`：跳过预览和确认，直接删除喵。
- `--recursive, -r`：递归扫描所有子目录喵。
- `--help, -h`：显示帮助喵。

### 安装

运行仓库根目录下的 `install.ps1` 脚本即可完成安装喵。

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

安装后会把程序复制到当前用户的 `PATH` 可用目录里喵。

### 发布

执行 `dotnet publish` 后，会自动在 `bin/Release-Archives/` 下生成压缩包喵。

---

## English

A command-line tool for cleaning camera `NEF` files.

It deletes `NEF` files that do not have a matching `JPG` counterpart.

### Features

- Scans the current directory by default.
- Use `--recursive` or `-r` to scan subdirectories.
- You can set separate directories for `NEF` and `JPG` files.
- `JPG` files are never modified.
- By default, the tool shows a deletion preview and asks for confirmation unless `--quiet` or `-q` is provided.

### Usage

```bash
neff [--nef-dir <path>] [--jpg-dir <path>] [--quiet] [--recursive]
```

### Options

- `--nef-dir, -n`: Path to the `NEF` directory, defaults to the current directory.
- `--jpg-dir, -j`: Path to the `JPG` directory, defaults to the current directory.
- `--quiet, -q`: Skip preview and confirmation, delete immediately.
- `--recursive, -r`: Scan all subdirectories recursively.
- `--help, -h`: Show help.

### Installation

Run the `install.bat` script from the repository root or from the published package root.

```bat
install.bat
```

The script installs the tool into a user-local directory and adds it to `PATH`.

### Publishing

After `dotnet publish`, the build will create a zip archive under `bin/Release-Archives/`.
