#DynamicTimelineFramework 2
#Under Construction

This framework is a continuation of a proof of concept project I made before (see Paradawn). The idea is to create a framework and library that manages a boundless virtual timeline that can persistently manage separate resolution paths (think of it like a multiverse). 

What makes this unique is the ability to resolve the state of any object on the timeline in an O(1) worst case access time, where n is defined as the "point" of the timeline being accessed, and an O(nLogn) worst case access time where n is defined as the number of "splits" in the resolution paths.

The main procedural principle that makes this work is loosely inspired on the work of [Maxim Gumin](http://github.com/mxgmn) and the [WaveFunctionCollapse](http://github.com/mxgmn/WaveFunctionCollapse) algorithm.

This improves on my original proof of concept version in a couple of ways:

* A timeline object is more compatible with the entity-component model to be more compatible with game design principles, instead of being polymorphic.
* Timeline state change times are completely open and flexible - no need to apply 2^k increments
* Resolution Paths will be defined by a daisy chain of "Diff" objects that more concretely define them in terms of what exactly has changed over the timeline.