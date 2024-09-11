# What is this code
This codebase is simply a backup repositry of the script assets of my own Unity project.
The version control is normally done via [Plastic SCM](https://www.plasticscm.com/) as it is fully integrated into the Unity Engine editor.
Plastic SCM did not however have a simple/straight-forward way of presenting codebases, hence why theres now a GitHub repo.

### How to read the code
These scripts work by being attached as components to a GameObject in Unity then ran in parallel as an instance of script, see [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html).
For the purpose of code reviewing this project, I would say the entry points are UIMainMenu.cs and Behaviour.cs.

UIMainMenu script is attached to a plain (no visual component) GameObject that contains a UI component that defines HTML like code for the component to render. For the moment, the UI encapsulates simple
functionality to create a barebone save file from which the user can switch scenes to the proper game scene, called "PrologueWolrd".

The *Prologue World* scene is were the game proper occurs. This scene is loaded with a single PCBehaviour (derived from Behaviour) and multiple NPCBehaviour's (also derived from Behaviour).
I recommened reading the codebase from the reference point of the Behaviour class since all gameplay interactions occur from the reference point of these two subclasses, PC- and NPC-Behaviour.
