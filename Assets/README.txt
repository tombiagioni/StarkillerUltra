----------------------------------------------------------
 Danmaku Engine for Unity, v1.0
 
 (c)2015, Virtual Dropkick - http://virtualdropkick.com
----------------------------------------------------------


NOTE
----------------------------------------------------------

This document merely serves as a very basic description
of this plugin. We highly recommend reading the full
online documentation.

* For a full description of the Danmaku Engine, visit:
    http://virtualdropkick.com/danmaku-engine/manual

* For the full documentation of DanmakuJSON, visit:
    http://virtualdropkick.com/danmaku-engine/danmakujson

* You can also check out the showcase to see the plugin
  in action:
    http://virtualdropkick.com/danmaku-engine/showcase  

If you already imported this plugin to your project,
you can also select "Help -> Danmaku Engine Manual" from
the Unity menu to access the online documentation.

----------------------------------------------------------



 TABLE OF CONTENTS
-------------------

 1. Introduction
 2. Requirements
 3. Installation
 4. Configuration 
   4.1 The "DanmakuController"
   4.2 Setting up contexts
   4.3 Creating Prefabs for bullets & emitters
   4.4 Bullet Libraries
   4.5 Setting up a "DanmakuOrigin"
   4.6 Creating bullet patterns
   
 5. Scripting API
 6. DanmakuJSON
 7. Time, angles & units 
 8. Support / Feedback
 9. Change Log
10. MIT License



 1. INTRODUCTION
-----------------

The Danmaku Engine is a plugin for Unity to easily create complex
bullet-patterns for 2D "bullet-hell" (jap.: "danmaku") games. It can also be
used for spawning and controlling the movement of enemies, power-ups, etc.

Even though the engine is primarily designed for arcade-style Shoot'em ups, it 
basically works like a programmable 2D-particle system which can be used for
many other game-genres or anything which requires complex movement of many
objects.


Main Features:
--------------

- Easy creation of complex movement-patterns using DanmakuJSON
- Live-editing of patterns inside Unity for rapid creation and debugging
- High flexibility
- Ready to use without any programming


Note, that this document merely serves as a very basic
description of this plugin. We highly recommend reading the full
online documentation.

* For a full description of the Danmaku Engine, visit:
    http://virtualdropkick.com/danmaku-engine/manual

* For the full documentation of DanmakuJSON, visit:
    http://virtualdropkick.com/danmaku-engine/danmakujson



 2. REQUIREMENTS
-----------------

This plugin requires Unity 4.5.2f1 or higher.



 3. INSTALLATION
-----------------

* For a more detailed description of the installation process, see:
    http://virtualdropkick.com/page/danmaku-engine-unity-package

After downloading the Unity Package, you can import it in your project.
We recommend using an empty project first to get started.

The folder "/Plugins/DanmakuEngine" *must* be imported entirely in
each of your projects which use the Danmaku Engine. However, the
example project (located in "/DanmakuExample") can be omitted if
you don't want to import it.

If you're just getting started, we recommend importing the example
project (and opening the scene /DanmakuExample/Scenes/Example.unity)
which will make it easier for you to follow along with this document.

You can always access the online manual by choosing
"Help -> Danmaku Engine Manual" from the Unity menu.


   
 4. CONFIGURATION
------------------

This plugin comes with an example project which is already setup
to work out-of-the-box but if you're starting with an empty project
you have to follow some required steps to make the Danmaku Engine
work.



 4.1 THE "DanmakuController"
--------------------------- 

* For a more detailed description of the setup process, see:
    http://virtualdropkick.com/page/danmaku-engine-danmakucontroller

The DanmakuController is the global interface to the Danmaku Engine
and it defines essential parameters.

Every scene which uses the Danmaku Engine requires *exactly one*
GameObject with the DanmakuController-component attached to it.

To add the DanmakuController-component to a GameObject, select it
first and then choose "Component -> Danmaku Engine -> DanmakuController"
from the menu.   

You can also add the component from the inspector of the currently selected
GameObject by clicking "Add Component -> Danmaku Engine -> DanmakuController".

The name and location of this GameObject are not important - just choose a
meaningful name and an appropriate location.

To make sure that the DanmakuController-component is initialized properly
before you access it with your own code, you should set its execution-order
accordingly.


Properties of the DanmakuController-component:
----------------------------------------------
* Use Frame Based Time
    Determines if all time-dependent calculations are based on seconds or
    frames.

* Do Not Destroy On Load
    Determines if this GameObject will be destroyed when another scene is
    loaded or not.

* Contexts
    A set of contexts which define essential parameters.



 4.2 SETTING UP CONTEXTS
-------------------------

* For a more detailed description of the setup process, see:
    http://virtualdropkick.com/page/danmaku-engine-danmakucontroller#contexts

All bullets and emitters operate on a specific context, which is controlled by
the DanmakuController to determine their behaviour. To make the Danmaku Engine
work, you have to define *at least one* context but you can define as many as
you want. This is useful if you want to specify certain parameters for regular
enemies, bosses and players independently, for example.


Properties of a DanmakuContext:
-------------------------------

* Name
    The name of this context which will be used for identification.
    Note, that every context needs a unique name.
    Also note, that context-names are case-sensitive.

* Bullet Library
    A reference to the bullet library which will be used by this context.

* Time Scale
    Defines the time scale (usually 1.0) of all bullet-sources that are
    operating on this context. This value can be used for slowdown- or
    speedup-effects as well as debugging purposes.
    Note, that this value scales with Unity's Time.deltaTime if the
    DanmakuController is using second-based timing.

* Targets
    A set of Transforms that will be used to determine targets which your
    bullets can be aimed at.
    Targets are mainly used by the aim-action in DanmakuJSON.
    Note, that you can register and unregister targets at runtime.

* Bullet Container
    A reference to a Transform in your scene which will be used as the
    parent of all spawned bullet-sources.
    Note, that you don't have to define a bullet container but it's
    recommended to keep your object hierarchy clean at runtime.

* Global Variables
    A set of named values which can be used in DanmakuJSON's Terms.
    In the example project, we defined the variable "rank" which can be
    used in bullet-patterns that scale dynamically with your game's
    difficulty (several danmaku-games do that).
    Note, that you can also define your own functions for resolving
    global variables.




 4.3 CREATING PREFABS FOR BULLETS & EMITTERS
---------------------------------------------

* For a more detailed description of the setup process, see:
    http://virtualdropkick.com/page/danmaku-engine-bullets-emitters
    
You can freely create Prefabs for your bullets - the only requirement is
that you have to attach the DanmakuBullet-component (or a component which
derives from it).

To add the DanmakuBullet-component to a GameObject, select it first and
then choose "Component -> Danmaku Engine -> DanmakuBullet" from the menu.

You can also add the component from the inspector of the currently selected
GameObject by clicking "Add Component -> Danmaku Engine -> DanmakuBullet".

For a classic 2D-Shoot'em up, you would usually add a SpriteRenderer and a
BoxCollider to your bullets.

Emitters are usually invisible objects which serve as the spawn-point for
bullet-sources. Therefore, you normally need just one empty Prefab with the
DanmakuEmitter-component attached to it.

To add the DanmakuEmitter-component to a GameObject, select it first and then
choose "Component -> Danmaku Engine -> DanmakuEmitter" from the menu or
"Add Component -> Danmaku Engine -> DanmakuEmitter" from the inspector.

Even though emitters are usually not visible, you can add any kind of visual
elements if you want to, of course.



 4.4 BULLET LIBRARIES
----------------------

* For a more detailed description of the setup process, see:
    http://virtualdropkick.com/page/danmaku-engine-bullet-libraries

Bullet libraries serve as catalogues for your bullet- and emitter-presets
to identify them by a name and to group them in distinct units.

You can create a bullet library by selecting the option
"Assets -> Create -> Danmaku Engine -> Bullet Library" from the main-menu or
"Create -> Danmaku Engine -> Bullet Library" from the context-menu in the
project hierarchy (right-click in the "Project"-window).

You can add as many emitter- and bullet-presets as you want but note that
you have to define *at least* the corresponding default presets.


Properties of a BulletLibrary:
------------------------------

* Default Emitter
    This preset defines the emitter which will be used by default.

* Emitter Presets
    A set of additional emitter presets which can be referenced
    in DanmakuJSON.

* Default Bullet
    This preset defines the bullet which will be used by default.

* Bullet Presets
    A set of additional bullet presets which can be referenced
    in DanmakuJSON.


Every preset consists of the following properties:

* Key
    The unique identifier of this preset which can be referenced
    in DanmakuJSON.
    Note, that the identifiers only have to be unique inside the
    corresponding bullet library.

* Prefab
    A reference to the Prefab which will be spawned during runtime.
    Note, that emitter-presets require a GameObject with an attached
    DanmakuEmitter-component and bullet-presets require the
    DanmakuBullet-component.

* Destroy When Invisible
    Determines if the GameObject will automatically be destroyed once
    it became invisible.
    Note, that this parameter only takes effect if you have one of
    Unity's renderer-components (e.g. SpriteRenderer or MeshRenderer)
    attached to your Prefab.


How bullet- and emitter-presets are determined
----------------------------------------------

Unless you don't override the default behaviour of your DanmakuContexts,
the keys of your bullet-sources as defined in your DanmakuJSON-files
will be used to identify a preset.

However, if you define a bullet-source which you want to refer to a
preset with a different key, you can add the property "preset" in your
DanmakuJSON-file.

If no preset with the provided key can be found, the default-preset you
have defined in your bullet librabry will be used.

* For the full documentation of DanmakuJSON, visit:
    http://virtualdropkick.com/danmaku-engine/danmakujson



 4.5 SETTING UP A "DanmakuOrigin"
--------------------------------

* For a more detailed description of the setup process, see:
    http://virtualdropkick.com/page/danmaku-engine-danmakuorigin

Every object which you want to spawn bullets (e.g. an enemy, the muzzle
of a stationary gun or your player's cannons) needs the
DanmakuOrigin-component attached to it.

To add the DanmakuOrigin-component to a GameObject, select it first and
then choose "Component -> Danmaku Engine -> DanmakuOrigin" from the menu.

You can also add the component from the inspector of the currently selected
GameObject by clicking "Add Component -> Danmaku Engine -> DanmakuOrigin".

Next, you should define which context this origin will operate on and which
bullet patterns it uses.


Properties of a DanmakuOrigin:
------------------------------

* Context Name
    The name of the context which the origin (and all its spawned
    bullets/emitters) will operate on.
    Note, that context-names are case-sensitive and have to exactly match
    the names you have assigned in your DanmakuController-component.

* Bullet Pattern
    A reference to a compiled DanmakuJSON-file

* Bullet Patterns
    A set of additional compiled DanmakuJSON-files

* Start Pattern Automatically
    If you have a DanmakuJSON file attached to the field "Bullet Pattern",
    this origin will start the bullet pattern as soon as it becomes active.



 4.6 CREATING BULLET PATTERNS
------------------------------

* For a more detailed description of the setup process, see:
    http://virtualdropkick.com/page/danmaku-engine-create-bullet-patterns

* For the full documentation of DanmakuJSON, visit:
    http://virtualdropkick.com/danmaku-engine/danmakujson
    
Bullet patterns are described in DanmakuJSON-files (with the extension .dmjson)
which can be edited in any text editor.

You can create a new bullet pattern by selecting the option
"Assets -> Create -> Danmaku Engine -> DanmakuJSON File" from the main-menu or
"Create -> Danmaku Engine -> DanmakuJSON File" from the context-menu in the
project hierarchy (right-click in the "Project"-window).

You can also put .dmjson-files anywhere in the /Assets-folder of your Unity
project - these will be imported automatically.

To avoid performance penalties during runtime, all .dmjson-files are
automatically compiled to a binary format which is stored in additional
.asset-files. Note, that only these compiled files can be attached to
your DanmakuOrigins.

In case you deleted one or more compiled DanmakuJSON-files choose the menu
option "Edit -> Danmaku Engine -> Compile All Bullet Patterns..." to
re-compile all .dmjson-files in your project.


The DanmakuJSON Editor
----------------------

For quick live-editing and debugging purposes you can edit your bullet
patterns with the DanmakuJSON Editor (also while your game is running).
Choose "Window -> DanmakuJSON Editor" to open it.

Note, that this editor has a very limited feature set - use a text editor
you are most comfortable with if you want to make bigger changes to your
bullet patterns.



 5. SCRIPTING API
------------------

The Danmaku Engine is designed in a way which doesn't require any
programming in order to work but you can customize its behaviour to fit
your individual needs.

* For the full documentation of the scripting API, visit:
    http://virtualdropkick.com/page/danmaku-engine-api



 6. DanmakuJSON
----------------

The Danmaku Engine uses a slightly modified JSON-format for the definition
of bullet-patterns, called "DanmakuJSON".

If you are not familiar with JSON, we recommend to check out its description
at http://json.org

There are four things that make DanmakuJSON different from the standard
JSON-format:

    1. Even though the root element of a DanmakuJSON-file is an object, it's
       not enclosed by curly brackets.
       
    2. Names/keys on the left-hand-side don't have to be enclosed by quotation
       marks
       
    3. You can write comments by using # as the leading character of a line
    
    4. The file extension is ".dmjson"

    
* For the full documentation of DanmakuJSON, visit:
    http://virtualdropkick.com/danmaku-engine/danmakujson



 7. TIME, ANGLES & UNITS 
-------------------------

Read the following sections carefully to fully understand how certain values
are handled in the Danmaku Engine.


Angles & rotations
------------------

All angles and rotations are measured in degrees from zero (up) to 360
(clock-wise).


Units
-----

All position-related values are internally handled as floats which are directly
translated to Unity's world units.


How time is handled
-------------------

Everything which is time-based is calculated in seconds - therefore, every
time-step is based on Unity's Time.deltaTime by default, because many
Unity-projects are built frame-rate independent.

However, if you want the calculations to be frame-dependent you can enable the
checkbox "Use Frame Based Time" in the inspector of the DanmakuController in your
scene. This will internally set the time-delta for each frame to 1.0, which
mathematically turns all calculations to "per-frame" instead of "per-second".

Keep in mind that many numerical values you are using in your bullet-patterns
depend on the used time-calculation, so you should decide beforehand which one
you want to use. For example, if you define a bullet with a speed of 5 and you
are using time-based calculations, this bullet will travel 5 distance-units per
second - if you set the timing to frame-based, it will travel 5 distance-units
per frame which is a significant difference.



 8. SUPPORT / FEEDBACK
-----------------------

* If you encounter any problems, please contact:
    support@virtualdropkick.com

* If you want to give us any kind of feedback, please contact:
    danmaku@virtualdropkick.com



 9. CHANGE LOG
---------------

v1.0
    - Initial release



 10. MIT LICENSE
-----------------

The Danmaku Engine uses a modified version of Dan Manning's
Equationator, which is released under the MIT License.

* https://github.com/dmanning23/Equationator

The MIT License (MIT)

Copyright (c) 2013 Dan Manning

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
