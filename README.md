# Spline-Editor - Technical Design I Final Project
Spline Editor Implementation for Unity3D via Editor Scripts. Lets the user create and edit splines directly in editor.



## Documentation 

You can find the documentation [here](https://iknightess.github.io/SplineEditor_Documentation/annotated.html)

## Class Summary
**CurvePoint** represents a single point in the spline. Has an anchor point and two control points.

**Spline** has a list of CurvePoints and functions to add, remove, insert and interpolate through these.

**SplineCreator** is a short class to create a new spline, linked to SplineEditor script.

**SplineEditor** is an Editor Script, contains the logic for drawing the points and the curve. Implements buttons for adding, removing and inserting points.

**SplineFollow** is a sample application of the Spline Editor, when added to an object, the object will follow the curve.



## Features

### Handles for Anchor Points:

![Handles for Anchor Points](https://media.giphy.com/media/20EEC880DStbaaKYB9/giphy.gif)

### Click-Drag for Control Points:

![Click-Drag for Control Points](https://media.giphy.com/media/OqBVjsP4CERRoE57Jj/giphy.gif)

### Global/Local Space Responsive:

![Global/Local Space Responsive](https://media.giphy.com/media/xFlwjzXmvDytmKSZKn/giphy.gif)

### Add point to End:

![Add point to End](https://media.giphy.com/media/3r9HnxC9TQt6J2rn4Y/giphy.gif)

### Insert Point after Active Point:

![Insert Point after Active Point](https://media.giphy.com/media/1n75UrE44f5WSQx0BX/giphy.gif)

### Remove Active Point:

![Remove Active Point](https://media.giphy.com/media/1Rkt8dsRLvMsHCMTTP/giphy.gif)

### Looping Splines:

![Looping Splines](https://media.giphy.com/media/DB4XZ3uzNkEfcn1stF/giphy.gif)


## Final Product - Objects following the Spline Paths

![Final Product](https://media.giphy.com/media/tKHNFMw2Ny0md16ceG/giphy.gif)

