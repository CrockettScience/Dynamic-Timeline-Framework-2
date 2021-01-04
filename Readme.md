# DynamicTimelineFramework 2
### Overview

This library provides a framework for procedurally generating near-boundless (2^64) timelines over any set of objects with programmable, discreet states. Finding the objects state, represented by a Position object, at any given time t is a CONSTANT time process in terms of t. In other words, it takes the same amount of time to find the state of any object at time 10 as it takes to find it at time 10 billion.

**Objects** in the timeline are objects that inherit from abstract class "**DTFObject**." An object represents a handle, and the definition requires a set of special attributes. Objects all define a set of possible positions the object can be in at any given time. The object can also set "Lateral" and "Parent" objects, both of which are informed of constraints and changes that occur in the timeline of the object, and who's states therefore are inextricably "tied" to the states of this object.

The state of an object at any given time is called a '**Position**'. A Position can represent a single state, or it can represent a "superposition" of several states, any of which is a "possible" Position the object can have at that point. Positions, therefore, have a measure of "uncertainty" associated with them, and a value of "0" represents a fully collapsed state. This quantum-mechanics-like mechanism is not unlike the driving mechanism behind [Maxim Gumin's famous "WaveFunctionCollapse" algorithm](https://github.com/mxgmn/WaveFunctionCollapse).

Additionally, this framework is DYNAMIC: meaning you can CHANGE states in the timeline, and the timeline will accomodate those changes. It does this using separate but parallel **Universes**, structured like a tree. Any given universe shares the states of all the objects up to the time of separation, where the universe's timeline "branches off" of it's parent.

Universes are contained by an object called a **Multiverse**. A Multiverse is an object that represents the collection of parallel universes, and is the first object that needs to be created when running the framework. The multiverse is initially created with 1 "Base" universe, which all parallel universes will branch from.

The primary form of interaction with the timeline comes from objects called "**Continuities**." A Continuity is an object that can be created by a universe that represents a single object's timeline in that universe. All objects are created in "full" superposition, meaning that there is no certainty to any of the states. In order to actually get those states, you must "Constrain" them, using a Continuity. The Constrain() method will need a date, a position to constain (That may or **may not** be a possible Position for the date), and an output variable.

The output variable of the Constrain() method is a "**Diff**" object. When the Constrain() method attempts to constrain to a Position or SuperPosition that is not possible, but the Position given is transitionable from the Position at Date - 1, a Diff object is created and assigned to the output variable. A Diff object represents the "difference" between the universe that Continuity belongs to and a potential new parallel universe that embodies that difference. Therefore, Diff's are used by the Universe constructor, and upon doing so, the universe becomes part of the multiverse.

### On The Order of Growth

The important benefit to this framework is that, as mentioned before, operations have virtually constant O(n)'s in terms of finding what the state of an object is on the timeline. However, there is some changes in the order of growth of the number of Objects and the number of Universes. The set of graphs below illustrate the various T(n) operations measured on my machine.

![T(n) graphs](https://i.imgur.com/KFbEC3N.png)

The speeds for these operations are SLOW in the very short run. While V2 is much more flexible, it comes at a huge cost of efficiancy. Maintaining lage flags and arrays of flags across the spans makes even small operations costly. V1 had the benefit of the tree like nature of the object relationships, which allowed a "lazier" flow of information. The system needs to be reengineered in such a way that the open object ralationship graph is reorganized into a tree in order to abuse the efficiencies that trees have. See [DynamicTimelineFramework 3](https://github.com/CrockettScience/Dynamic-Timeline-Framework-3).
