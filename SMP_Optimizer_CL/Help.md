---
html:
  embed_local_images: true
  embed_svg: true
  offline: false
  toc: undefined

print_background: false

export_on_save:
  html: true
---

@import "help_files/help.css"
# SMP Optimizer Help

## Table of Contents {ignore=true}

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->
  
<!-- code_chunk_output -->

- [How to use](#how-to-use)
  - [Method 1 (creates new SMP configuration files)](#method-1-creates-new-smp-configuration-files)
  - [Method 2 (overwrites SMP configuration files)](#method-2-overwrites-smp-configuration-files)
- [Optimization levels](#optimization-levels)
  - [Level 0 - Expensive](#level-0---expensive)
  - [Medium quality levels (1a & 1b)](#medium-quality-levels-1a--1b)
  - [Level 2 - Aggresive](#level-2---aggresive)
- [Analyze](#analyze)
  - [How to report results](#how-to-report-results)
  - [Analyzing and you](#analyzing-and-you)
- [Command line arguments](#command-line-arguments)

<!-- /code_chunk_output -->



## How to use

### Method 1 (creates new SMP configuration files)

This is the suggested way to use this program, since it will create new configuration files for your armors so the originals remain untouched.
If you use any kind of mod manager (and you should), you may prefer to use this method.

Only 2 steps are needed.

#### Step 1
Create a new file named `output_dir.txt` inside the folder where `SMP_Optimizer_CL.exe` is and **on the first line** of it write the folder name where you want your optimized files to be written to.

Save that file.

!!!
    This step is needed only the first time you run the program.

    Once `output_dir.txt` is properly configured, you can go directly to step 2 all subsequent times.

This is an example of how that could be done for MO2:

![output_dir.txt example](help_files/output_dir%20-%20folder.png)

 !!!info Exporting to *.zip
    You can also export the optimized files a zip file.

    To do so, just write a path to a zip file instead of a folder inside `output_dir.txt`.

    ![output_dir.txt example](help_files/output_dir%20-%20zip.png)


!!!warning 
    This program can only create zip (`*.zip`) files. 
    7z (`*.7z`) files are not supported for now.

#### Step 2

Drag your folder(s)/file(s) over any of the included `*.bat` files.

![Drag and drop on bat](help_files/bat%20drop.png)

Read more about optimization levels [here](#optimization-levels).

!!!warning 
    If you skipped step 1, the program will complain to you about not having a path to write its output and will automatically open a newly created `output_dir.txt` for your convenience.

    ![output_dir.txt example](help_files/asking%20for%20bat%20output.png)

### Method 2 (overwrites SMP configuration files)

This is the way the program worked in its first version. It will overwrite the original files with the optimized ones, so you may not want that if you're using a mod manager.

Just drag and drop your folder(s)/file(s) to `SMP_Optimizer_CL.exe`.

![drop on app](help_files/app%20drop.png)

Unlike the first version of this program, which ran the most aggresive method, this one will optimize your files using the [medium quality](#medium-quality-levels-1a--1b) method `1a`.

##  Optimization levels

SMP Optimizer can optimize your SMP files in these levels:

* **0 - Expensive**: almost no optimization; high quality.
* **1a - Vertex on triangle collisions**: medium quality.
* **1b - Triangle on vertex collisions**: medium quality.
* **2 - Aggresive**: poor quality.

!!!info TL;DR
    Use: 
    * `2` for small accessories/chains/earrings[^chainWarning], short clothes, most hairdos and things that generally go over the head [^notMasks].    
    * `1a` most of the time. `1b` if you think `1a` looks worse than expected.
    * `0` if everything else fails.

[^notMasks]: But maybe not things that go over the face, like veils, though.

[^chainWarning]: Please notice that this is only valid for accessories that are a single armor piece and such.

### Level 0 - Expensive

This is the default for almost all armors out there (at the time of writing, I've never seen anything but this).

Calculations are high quality but expensive, which means clothes will clip as you would expect and your CPU fans may run like jet engines while you play Skyrim.

Virtual bodies and cloths will use triangles (which are computationally expensive) for collision detection.

**You can still expect this level to make some gains**, since it changes virtual floor calculations from triangle (expensive) to vertex (cheap).
All armors I've seen use triangle collisions for the floor, by the way.

### Medium quality levels (1a & 1b)

These will be your bread and butter, since they configure you armors in such a way clipping is reasonable and cloth collisions are ok as well.

In fact, ***level 1a is the default***. 
That's the quality the program will optimize to when you drag and drop your files directly to it.

##### What's the difference between these two? 

In technical terms, `1a` makes the body to be calculated as triangles and armors as vertices.
`1b` is body as vertices and armor as triangles.

**In practice, I don't know**.
My flawed perception tells me there is no difference both in speed and CPU heath, but if someone knows how to test bencharks for this, I would really wish to know.

I personally suspect `1a` is better when armors have too many polygons.

Still, if you feel the default `1a` level has poorer quality than you expect, try `1b`.

### Level 2 - Aggresive

This mode is the one recommended by [FPS smashed by HDT SMP][] and it's the one that gave birth to SMP Optimizer.

It is quite aggresive and makes everything faster, but collisions can get **REALLY poor**, to the point that clothes always clip through bodies.

Still, it has its uses.

You can use this mode for most hairdos and small accesories like earrings and such, or even some short pieces of cloth.

<!-- ============================================== -->
@import "help_files/analyze.md"

<!-- ============================================== -->
## Command line arguments

This reference will be helpful if you want to modify the included `*.bat` files to suit your preferences or if you are one of those weirdos who like to run programs from the command line :stuck_out_tongue:.

### Inputh paths {ignore=true}
This program accepts both `*.xml` files and directory paths in any combination and number. 

Examples:

```
SMP_Optimizer_CL.exe "armor1.xml"
SMP_Optimizer_CL.exe "armor1.xml" "armor2.xml"
SMP_Optimizer_CL.exe "dir1" "dir2"
SMP_Optimizer_CL.exe "armor1.xml" "armor2.xml" "dir1" "armor3.xml"
```

Directories and subdirectories are always processed in a recursive manner, so you only need to input the root path if you want to process all the files and directories inside the root.

### Flags {ignore=true}

| Flag            | Effect                                                                                                                                                                       |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `-h`            | Opens this file[^HelpLol].                                                                                                                                                   |
| `-t`            | Runs in *testing mode*. <br>Everything will be optimized as expected, but no files will be created/overwritten.                                                              |
| `-v`            | Verbose mode. <br>Writes a lot of detailed info, so you know exactly what the program is doing. <br>Useful for bug reporting.                                                |
| `-bat`          | Makes some extra validations when the program was run from a `*.bat` file (not meant to be used by users). <br>**Don't delete this from any of the included `*.bat` files**. |
| `-l0`           | [Expensive optimization](#level-0---expensive) level.                                                                                                                        |
| `-l1a`          | [Medium optimization](#medium-quality-levels-1a--1b) level. Vertex on triangle collisions. <br>**Used by default** if no optimization level is provided.                     |
| `-l1b`          | [Medium optimization](#medium-quality-levels-1a--1b) level. Triangle on vertex collisions.                                                                                   |
| `-l2`           | [Aggresive optimization](#level-2---aggresive) level.                                                                                                                        |
| `-o "dirOrZip"` | Output to some directory or `*.zip` file. **Path must always follow `-o`**. <br>If this argument is not provided, **optimized files will overwrite the original ones**.      |

All flags and inputs can be used in any order; the only limitation being `-o`, which should be always followed by  `"dirOrZip"`.
If you add `-o` with no output path, SMP Optimizer will overwrite processed files:

```
SMP_Optimizer_CL.exe -l2 "input1" "input2" -o
```
Here are some examples of normal usage:

```
SMP_Optimizer_CL.exe -l1a "input1" "input2" -t
SMP_Optimizer_CL.exe "input1" "input2" -l0 -v
SMP_Optimizer_CL.exe -o "output.zip" "input1" "input2" -l1b
SMP_Optimizer_CL.exe "input1" -o "output dir" "input2" -l1b
```

[^HelpLol]: Did you really expect to see this table inside the console app? Lol. It's 2024, dude :stuck_out_tongue:.

[FPS smashed by HDT SMP]: https://www.nexusmods.com/skyrimspecialedition/mods/25001