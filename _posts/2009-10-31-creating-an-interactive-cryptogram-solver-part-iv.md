---
layout: post
title: Creating an interactive cryptogram solver (Part IV)
date: 2009-10-31 19:40:25.000000000 -04:00
tags:
- ACA
- aristocra
- cryptography
- cryptograms
- programming
- python
- solver
permalink: "/2009/10/31/creating-an-interactive-cryptogram-solver-part-iv/"
---
![Execute Interactive Solver]({{ site.baseurl }}/assets/2009/09/solver.png "Execute Interactive Solver")

I've been working hard on this part of the series because I really wanted the interactive cryptogram solver to make it quick and painless to jump right into solving and still give you plenty of room to expand the functionality and reflect your own style of solving. In this part of the series, we will create solver.py which will become our gateway to solving. It will allow us to quickly select a cipher class that we want to work with. We will also add a self documenting system that will allow us to use the solver without memorizing all the commands or shortcuts that each solver class may use. So, lets just jump right back into the code!

<!--more-->
<a name="more" />

#### Clearing the screen

While we are using the solver, it is a common place for the screen to get a little crowded. So lets add a new function to our shared.py that will take care of clearing the screen. Clearing the screen is a little complicated because each operating system has it's own way of handling it. Our _clear_screen_ function will handle all the major players: Windows, Mac, and Linux.

Contents of shared.py (Place at the bottom of the file)

```python
def clear_screen():
  import os
  if os.name == "posix":using the
      # Unix/Linux/MacOS/BSD/etc
      os.system('clear')
  elif os.name in ("nt", "dos", "ce"):
      # DOS/Windows
      os.system('CLS')
```

All we are really doing here is importing the _os_ and using the _os.name_ to tell which operating system we are in and then executing the appropriate command for clearing the screen.

#### The Solving Gateway: solver.py

I call _solver.py_ the gateway because this is really the file that you will be executing the most. Everytime we create a new class based on _CipherSolver_, we'll want to import it into _solver.py_ so we can access it. I'll show you how to do that in just a minute. First, lets just show the entire contents of _solver.py_ and then we'll go through the good parts.

Contents of solver.py

```python
import shared
from cipher import CipherSolver
from aristocrat import AristocratSolver
from patristocrat import PatristocratSolver

if __name__ == '__main__':
  solvers = {}
  for name, obj in globals().items():
    if name.endswith("Solver") and issubclass(obj, CipherSolver):
      solvers[name[:-6]] = obj

  while True:
    shared.clear_screen()
    print "-" * 37
    print "    Interactive Cryptogram Solver"
    print "-" * 37
    print "Type a cipher type or partial (q to quit) [all]: ",
    cmd = raw_input().lower()

    if cmd in ["q","quit"]:
      break
    else:
      list = []
      for name, obj in solvers.items():
        if name.lower().startswith(cmd):
          list.append(name)
      if len(list) > 0:
        list.sort()
        for index in range(1,len(list) + 1):
          print "  {0}. {1}".format(index, list[index-1])
        print "Type number of selection or return to go back: ",
        cmd = raw_input()
        if cmd.isdigit() and (0 < int(cmd) <= len(list)):
          name = list[int(cmd)-1]
          print "nLoaded {0} Solver.".format(name)
          solvers[name]().solve()
      print ""
```

The first few lines are essentially the most important part of this file. They import all the _CipherSolver_ classes that we want available. You only need to import the your Solver classes, not the Cipher classes.

```python
from cipher import CipherSolver
from aristocrat import AristocratSolver
from patristocrat import PatristocratSolver
```

Right now we only have three classes to import. We have to import the _CipherSolver_ class because everything is based on that one. This is where you would include any other Solver classes that you create. The pattern is:

```python
from FILENAME import SOLVERCLASS
```

Just add any new ones below the _PatristocratSolver_ line.

```python
if __name__ == '__main__':
  solvers = {}
  for name, obj in globals().items():
    if name.endswith("Solver") and issubclass(obj, CipherSolver):
      solvers[name[:-6]] = obj
```

Line 6 is an interesting line of code that you may have seen in other Python files. Python files can be executed on their own or imported from other Python files as we have seen above. One problem that arises is that python tries to execute all lines that are in the file as it is imported. But what if we want to execute code only when our file is executed on its own? That is exactly what this line of code does. It essentially says "If this file is being executed on its own and not imported, then do the following:". Anything indented below the "if statement" will only be executed when we type:

```
python solver.py
```

The rest of the lines in this section walks through all the imported classes and functions in the _globals_ dictionary. Whenever we use the _import_ command, the things we import get stored in the _globals_ dictionary. So here we are scanning _globals_ and pulling out any classes that are based off of our _CipherSolver_ class. We'll store all the classes and their names so we can use them later.

Note: I'm going to warn you right now, this part of the series contains a lot of code that is purely just for display purposes. We'll be formatting a lot of things things to display on the screen. I won't go over every line in these cases. Just know that if you see "print" or "output" then those are things that will be displayed or eventually displayed.

```python
    cmd = raw_input().lower()

    if cmd in ["q","quit"]:
      break
    else:
      list = []
      for name, obj in solvers.items():
        if name.lower().startswith(cmd):
          list.append(name)
      if len(list) > 0:
        list.sort()
        for index in range(1,len(list) + 1):
          print "  {0}. {1}".format(index, list[index-1])
        print "Type number of selection or return to go back: ",
        cmd = raw_input()
        if cmd.isdigit() and (0 < int(cmd) <= len(list)):
          name = list[int(cmd)-1]
          print "nLoaded {0} Solver.".format(name)
          solvers[name]().solve()
      print ""
```

Here we prompt the user to type in the name or part of a name of the class that they would like to use. We then walk through our list of Solver classes and display any that match the inputted name and display them on the screen. The user then gets to type the number that corresponds with the class on the screen. You can also just press enter to get a list of all available classes. Lets see what the looks like when we run it.

```
python solver.py
-------------------------------------
    Interactive Cryptogram Solver
-------------------------------------
Type a cipher type or partial (q to quit) [all]:
```

Lets just press enter here to see all the classes that are loaded:

```
  1. Aristocrat
  2. Cipher
  3. Patristocrat
Type number of selection or return to go back:
```

If we type "3" to select Patristocrat and then press enter we get the following:

```
Type number of selection or return to go back:  3

Loaded Patristocrat Solver.
>
```

Now we are ready to solve a Patristocrat! Our _PatristocratSolver_ class is automatically loaded and executed. If we want to go back to the selection screen we can just press Ctrl+C at any time.

#### The Documentation System

I don't know about you but I have a major problem remembering things sometimes. I even have a T-Shirt that says "Insufficient Memory" on it. So if we create a new solver class for the over sixty different cipher types used in the Cryptogram, there is no way I could remember all the different commands and functions that we would need. It would be very beneficial for us to have documentation on all the different classes we use and to be able to display it whenever we need to. I'm not the greatest at writing documentation especially when the code can change so quickly. So our documentation system would have to be easy to update and not super complicated. Alright, here we go. We are going to put the documentation into the _CipherSolver_ class so most of our changes are going to be in the cipher.py file.

Contents of cipher.py (Add at the top on line 4)

```python
import inspect,types
```

Here we are importing two modules. The _inspect_ module is going to be the core of our documentation. The python documentation describes this module as:

> inspect - Get useful information from live Python objects.

This module can give you all sorts of information about our classes and objects. We'll see how its used in a minute.

```python
class CipherSolver(object):
  def __init__(self):
    self.cipher = Cipher()
    self.prompt = ">"
    self.maxlinelen = 70
    self.shortcuts = {"?":"_display_help","d":"display"}
```

The above code is just slightly different from Part III. We are now telling the _CipherSolver_ that it should inherit from the _object_ class. This is the new style of classes in python and just makes sure our code will work in future version of python and that all the special python modules that use objects will work correctly. We also add a new shortcut on line 35. This shortcut lets us type "?" at any time to activate our help system by calling the __display_help_ function.

One thing to note here, any function that starts with a "_" will be hidden from the documentation system. They won't be displayed. This allows you to add helper functions that are not available for calling directly. Lets add a few of these little helper functions. (I'm going to go a little out of order here. If you want to see the cipher.py file in its correct order, view the source at the end of this article.)

```python
  def _inherit_docs(self, BaseClass):
    members = inspect.getmembers(self, inspect.ismethod)
    for name, method in members:
      if method.__doc__ == None and hasattr(BaseClass, name):
        method.im_func.__doc__ = getattr(BaseClass, name).__doc__
```

The __inherit_docs_ function will walk through all the different overridden functions in our classes and make sure they have their own documentation or use the documentation from the base class. This may sounds confusing at first but we'll show actual code examples later. Simply put, since all our classes are going to have a _display_ function that just does things a little different for each class, why should we have to repeat the documentation each time? In our documentation system, the _PatristocratSolver_ will look at the _AristocratSolver_ for its documentation and the _AristocratSolver_ will look at _CipherSolver_ for its documentation. Lets add those calls right now. Add the following to the aristocrat.py and patristocrat.py.

Contents of aristocrat.py (Add to the AristocratSolver.__init__ method)

```python
self._inherit_docs(CipherSolver)
```

Contents of patristocrat.py (Add to the PatristocratSolver.__init__ method)

```python
self._inherit_docs(AristocratSolver)
```

We need to make one major modification to the _CipherSolver.solve_ method. In the previous parts of this series, we made the _solve_ method handle any methods that are part of the solver class and the cipher class. Our modification is going to change this to only go through the solver class. This might sound like a disadvantage at first, but it will make our solver much more powerful in the end. Change your solve method to the following:

```python
  def solve(self):
    while True:
      try:
        raw_line = raw_input(self.prompt + " ")
        line = re.findall('\'[^\']*\'|\"[^\"]*\"|S+', raw_line)
        line = [item[1:-1] if item[0] in '\'"' else item for item in line]
        print ""
        cmd = line.pop(0)
        attr = None
        if cmd in self.shortcuts:
          cmd = self.shortcuts[cmd]
        if cmd.endswith("?"):
          print self._display_help(cmd[:-1])
        else:
          if hasattr(self, cmd) and cmd != "solve":
            attr = getattr(self, cmd)
          if attr == None:
            print "Unknown command: ", cmd
          else:
            retvalue = None
            if type(attr) == types.MethodType:
                retvalue = attr(*line)
            else:
              if len(line) == 0:
                retvalue = attr
              else:
                setattr(self, cmd, *line)
            if retvalue != None:
              print retvalue
          print ""
      except KeyboardInterrupt:
        print "Exiting Solver"
        break
      except Exception, e:
        print "Error: ", e
```

The big change here is that we first check to see if the command ends with a question mark. If so, it will pass the command, minus the question mark, to the _display_help function that we'll discuss in a minute. We also add the support to get and set a properties that are defined in the class.

Now comes the entire documentation system. Its all bundled in a special function called __display_help_. Now be prepared, this is a long function.

```python
  def _display_help(self, cmd=None):
    if cmd in self.shortcuts:
      cmd = self.shortcuts[cmd]

    def _get_function_def(name,function):
      specs = inspect.getargspec(function)
      defaults = None
      if specs.defaults:
        defaults = dict(zip(specs.args[-len(specs.defaults):],specs.defaults))
      output = name
      for arg in specs.args:
        if arg != 'self':
          output += " " + arg
          if defaults and arg in defaults:.  I
            output += "=" + repr(defaults[arg])
      return output.  I

    def _display_doc(obj):
      doc = ""
      if inspect.getdoc(obj):
        for line in inspect.getdoc(obj).split("n"):
          doc += "   " + line + "n"
        doc += "n"
      return doc

    cmd_shortcuts = dict([(v,k) for k,v in self.shor.  Itcuts.items()])
    obj = None
    if cmd == None:
      obj = self.__class__
    elif hasattr(self.__class__,cmd):
      obj = getattr(self.__class__,cmd)
    output = ""
    if inspect.isclass(obj):
      output = obj.__name__ + ":nn"
      output += _display_doc(obj)
      member_filter = lambda obj: inspect.ismethod(obj) or isinstance(obj,property)
      members = dict(inspect.getmembers(obj, member_filter)).keys()
      members = [name for name in members if not name.startswith("_") and name != "solve"]
      members.sort()
      output += "  Available Commands:n"
      for index in range(len(members)):
        member = members[index]
        if cmd_shortcuts.get(member):
          member += " [" + cmd_shortcuts.get(member) + "]"
        output += member.rjust(20)
        if (index + 1) % 3 == 0:
          output += "n"
    elif inspect.ismethod(obj) and cmd != "solve":
      output = cmd
      shortcut = dict([(v,k) for k,v in self.shortcuts.items()])
      if shortcut.get(cmd):
        output += " [" + shortcut.get(cmd) + "]"
      output +=":nn" + _display_doc(obj)
      output += "   Usage:n      " + _get_function_def(cmd, obj)
    elif isinstance(obj,property):
      output = cmd
      if obj.fset == None:
        output += " (Read Only)"
      if cmd_shortcuts.get(cmd):
        output += " [" + cmd_shortcuts.get(cmd) + "]"
      output += ":nn" + _display_doc(obj)
      output += "   To display the value:n      " + cmd + "nn"
      if obj.fset != None:
        output += "   To change the value:n      " + _get_function_def(cmd, obj.fset)
    else:
      output = "Unknown command: " + cmd
    return output.rstrip("n") + "n"
```

It looks daunting at first but is actually quite simple. If you remove all the display code (all the output statements) this is what you're left with: the function takes a command that you want to see the documentation for and displays it or if you pass in blank, it'll display all the commands that are available and any shortcuts that are defined. Ok, lets see how this works right now! Fire up sandbox.py for now:

```
python sandbox.py
> ?

PatristocratSolver:

  Available Commands:
         display [d]  frequency_list [f]            keys [k]
        print_counts             set [s]

>
```

So if we type "?" It'll show us all the commands! Notice that the shortcuts are listed in brackets next to the commands. This way you can see them if you have forgotten what they are. Now lets get back to the code. Lets look at a few of the important parts of the __display_help_ function.

Did you notice that the _print_counts_ function was listed in our command list from above? That isn't really a command that should be executed in the solver. It's one of those special helper functions. Lets change its name to __print_counts_ so that it will be hidden (Note: You will need to change where it is used in aristocrat.py and patristocrat.py too or it will give you an error.).

Lets chop up the __display_help_ function into the three types of objects that the command passed in can represent. First, a solver class like _AristocratSolver_. Second, a method of the class like _display_. Third, a property of the class.

##### Solver Classes

As we saw before, if no command is passed into the function, then a summary of all available commands is displayed. This will include any shortcuts and properties that are defined for the class. This will also include any commands that are inherited from other classes. Lets look at the code that makes this possible:

```python
      member_filter = lambda obj: inspect.ismethod(obj) or isinstance(obj,property)
      members = dict(inspect.getmembers(obj, member_filter)).keys()
```

First we create a quick little lambda function that returns True or false. We want to get a list of all methods and properties of our class, so the lambda function will return True only if the "obj" argument is a method or property. Next, we execute the _inspect.getmembers_ function and pass in the obj (which is our solver class at this moment) and our lambda function from above. In python, the _inspect.getmembers_ function would return the following:

```
>>> import inspect
>>> from patristocrat import PatristocratSolver
>>> member_filter = lambda obj: inspect.ismethod(obj) or isinstance(obj,property)
>>> inspect.getmembers(PatristocratSolver,member_filter)
[('__init__', ),
('_display_help', ),
('_inherit_docs', ),
('_print_counts', ),
('display', ),
('frequency_list', ),
('keys', ),
('set', ),
('solve', ),
('text', )]
>>>
```

It returns a big list of name/value pairs for all the members that the member_filter evaluated to True. It would be a much longer list if we had not passed in a filter function. The documentation system will use this list to display a nicely formatted display of commands that we can execute.

##### Methods

When a method name is passed into the function, the doc string for the function is displayed, followed by the appropriate way to call the function and what parameters to pass in. Doc strings are an important part of Python classes and functions. They help provide documentation automatically. Doc strings are placed on the line directly below the declaration of the method. They are started with three quotes """ and ended with three more quotes """ Lets add a doc string to the display method and then see how our documentation system will react to it.

```python
  def display(self):
    """Displays the current data on the screen."""
    print self.cipher.text
```

Here we have add just one line right below the declaration of the display method. The doc string can contain any information you want displayed. It can really be as long as you want and can span multiple lines. Now lets see how it works. Load up the solver and select Patristocrat.

```
Loaded Patristocrat Solver.
> display?

display [d]:

   Displays the current data on the screen.

   Usage:
      display

>
```

Thee display isn't as impressive as if we looked at the frequency_list function. Lets check that one out to better see what is displayed (Note: We haven't put in a doc string for the frequency_list function yet so you won't see that):

```
> frequency_list?

frequency_list [f]:

   Usage:
      frequency_list length=1 text=''

>
```

We can see the names and default values for the frequency_list. If we don't pass in anything (by just typing frequency_list and hitting enter), it will automatically pass in a length of 1 and use the internal text value. But we could pass in other lengths if we want to. You don't have to do anything special to get this to display nicely like this. The code for it is actually quite simple thanks to the _inspect_ module. The inspect code is in the __get_function_def_ method that we declared above.

```python
      output += "   Usage:n      " + _get_function_def(cmd, obj)
```

```python
    def _get_function_def(name,function):
      specs = inspect.getargspec(function)
      defaults = None
      if specs.defaults:
        defaults = dict(zip(specs.args[-len(specs.defaults):],specs.defaults))
      output = name
      for arg in specs.args:
        if arg != 'self':
          output += " " + arg
          if defaults and arg in defaults:
            output += "=" + repr(defaults[arg])
      return output
```

The _inspect.getargspec_ function takes a function as the argument and returns all the information about the function:

```
>>> specs = inspect.getargspec(PatristocratSolver.frequency_list)
>>> specs
ArgSpec(args=['self', 'length', 'text'], varargs=None, keywords=None, defaults=(1, ''))
>>>
```

The returned value is called a named tuple. It has all the little chunks of information that we can use. One of the cool things we can do is use the _zip_ function to attach the argument names back to their default values:

```
>>> defaults = dict(zip(specs.args[-len(specs.defaults):],specs.defaults))
>>> defaults
{'text': '', 'length': 1}
>>>
```

The rest of the code is just the fluffy display code that makes everything look nice.

##### Properties

Properties are a great way to get and set different values that our solver will use. We want to be able to get and set the _Cipher.text_ from within our solver class. Lets declare our new property in _CipherSolver_:

Contents of cipher.py (Add to the CipherSolver class)

```python
  @property
  def text(self):
    """The raw text value used for encoding and decoding."""
    return self.cipher.text

  @text.setter
  def text(self,value):
    self.cipher.text = value
```

The "@property" tells python that we want the method that follows it to be a property. So now whenever we use the "text" command, the cipher.text will be display. The "@text.setter" tells python that if we try to set the text property, it should change the cipher.text. We can create as many of these kinds of properties we want whenever we want to get and set special values. We'll probably use them a lot when working with out cipher types.

The display for properties is different from the classes and methods from above. Lets see what our new text property looks like in the documentation system and the solver. Lets load up sandbox.py:

```
> text?

text:

   The raw text value used for encoding and decoding.

   To display the value:
      text

   To change the value:
      text value

>
```

So it shows the doc string for the property, how to display it, and also had to change it. Lets display and set our property:

```
> text

GSRHRHZGVH GLUGSVVNVITVMXBYIL ZWXZHGHBHGVN

> text "THIS IS A TEST"


> text

THIS IS A TEST

>
```

Now we can easily set the text property whenever we want to start a new con. These properties are really easy to create and are very powerful. Python makes it very easy to enhance our solver with whatever we need to get the job done.

#### Document Everything!

At this point the implementation of the documentation system is complete. All we have to do now is just add doc strings to all our methods and properties in each of the solver classes.

##### CipherSolver (cipher.py)

We've already added the doc strings to the existing methods and properties in our CipherSolver but lets add one more method that will come in handy. The _clear_ method just clears the screen whenever called. Sometimes when you'd made a lot of changes and there is tons of data on the screen, it is refreshing just to clear the screen before you continue. Add the following code above the display method:

```python
  def clear(self):
    """Clears the screen"""
    shared.clear_screen()
```

##### AristocratSolver (aristocrat.py)

Since the _AristocratSolver_ class inherits from _CipherSolver_, we want to make sure that all our overridden methods like display get their doc strings set correctly so we don't have to repeat ourselves. We do this by just adding a call to the __inherit_docs_ method in the _AristocratSolver.__init___

```python
class AristocratSolver(CipherSolver):
  def __init__(self):
    CipherSolver.__init__(self)
    self._inherit_docs(CipherSolver)
```

We need to add a few bits of documentation to our existing methods inside the _AristocratSolver_ class. Add the following doc strings:

```python
  def frequency_list(self, length = 1, text = ""):
    """Displays counts for frequencies of characters"""

  def keys(self):
    """Displays the plaintext and ciphertext keys."""

  def set(self, ct, pt = "-"):
    """Sets the plaintext equivalent for each ciphertext character.
    You can enter multiple letters at a time.
    Enter a single dash '-' to set the plaintext characters to blank."""
```

##### PatristocratSolver (patristocrat.py)

_PatristocratSolver_ class inherits from _AristocratSolver_ so we need to call the __inherit_docs_ method in the _PatristocratSolver.__init__:_

```python
class PatristocratSolver(AristocratSolver):
  def __init__(self):
    AristocratSolver.__init__(self)
    self._inherit_docs(AristocratSolver)
```

#### Conclusion

That was a lot! Hopefully, you survived up to this point. This concludes the documentation system for the interactive solver. You can add your own methods to any of the classes and add doc strings to document things. In Part V, the next and last article in this series, we will include loading the cons from the Cryptogram Digital Con files (available at cryptogram.org for members).

#### Complete Source Code

You can download the complete source code to Part IV at: [Creating an interactive cryptogram solver (Part IV – Source)](http://www.codepenguin.com/2009/10/31/creating-an-interactive-cryptogram-solver-part-iv/solver_part_4/).

#### Related Posts:

[Creating an interactive cryptogram solver (Introduction)](/2009/09/17/creating-an-interactive-cryptogram-solver-introduction/)  
[Creating an interactive cryptogram solver (Part I)](/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/)  
[Creating an interactive cryptogram solver (Part II)](/2009/09/20/creating-an-interactive-cryptogram-solver-part-ii/)  
[Creating an interactive cryptogram solver (Part III)](/2009/09/22/creating-an-interactive-cryptogram-solver-part-iii/)