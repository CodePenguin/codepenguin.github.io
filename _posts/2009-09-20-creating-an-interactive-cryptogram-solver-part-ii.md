---
layout: post
title: Creating an interactive cryptogram solver (Part II)
date: 2009-09-20 21:49:19.000000000 -04:00
tags:
- ACA
- aristocra
- cryptography
- cryptograms
- programming
- python
- solver
permalink: "/2009/09/20/creating-an-interactive-cryptogram-solver-part-ii/"
---
![Execute Interactive Solver]({{ site.baseurl }}/assets/2009/09/Solver.png "Execute Interactive Solver")

In [Part I](/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/) of this series we started creating the framework for our solver by creating the _Cipher_ and _Aristocrat_ classes. You are probably thinking "This is a series about interactive solvers but this is all code!" Well, the classes inheriting from _Cipher_ will be the ones doing all the work in our solver. In this part of the series we will finally create the _CipherSolver_ class that will work with the _Cipher_ classes to interactively get the work done. So lets just jump right into the code so we can finally get to our first working solver, the _AristocratSolver_ class!

<!--more-->
<a name="more" />

**Disclaimer:** I just wanted to give you a heads up before you get into this part of the series. This interactive solver does not use the mouse for interaction. Commands or actions are typed into the program and the program will execute those actions. In the future I may create a new series of articles that describe a fully graphical (uses the mouse) version of the interactive solver. This series of articles is intended to help you create a simple and powerful interactive solver that can be used on any computer that can run Python. This means that it will work on Windows, Mac, Linux and most other major operating systems.

**Note:** We are going to be adding code to our existing files so I will not post the entire file contents like I did in Part I. I'll just let you know what has changed and where to put it.

#### The CipherSolver Class

The _CipherSolver_ class is going to be our base class for all Solvers. It will allow us to easy extend the functionality to fit whatever cipher type we are working on. We are going to need to import a few libraries for this class so lets add the following code to the top of "cipher.py":

Contents of cipher.py (Place at the top of the file)

```python
import types
import re
```

This just imports the "types" and "re" libraries so we can use them later in the code. At the bottom of the file we are going to create our _CipherSolver_ class.

Contents of cipher.py (Place at the bottom of the file)

```python
class CipherSolver:
  def __init__(self):
    self.cipher = Cipher()
    self.prompt = ">"
    self.shortcuts = {"d":"display"}

  def display(self):
    print self.cipher.text

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
        if hasattr(self, cmd):
          attr = getattr(self, cmd)
        elif hasattr(self.cipher, cmd):
          attr = getattr(self.cipher, cmd)
        if attr  None:
          if type(attr) == types.MethodType:
            retvalue = attr(*line)
          else:
            retvalue = attr
          if retvalue != None:
            print retvalue
        else:
          print "Unknown command: ", cmd
        print ""
      except KeyboardInterrupt:
        print "Exiting Solver"
        break
      except Exception, e:
        print "Error: ", e
```

There is a lot going on here so lets break it into chunks smaller chunks.

```python
  def __init__(self):
    self.cipher = Cipher()
    self.prompt = ">"
    self.shortcuts = {"d":"display"}

  def display(self):
    print self.cipher.text
```

In the Initialization section we create a variable that will hold an instance of our _Cipher_ class or one of its descendants, like the _Aristocrat_ class. The solver will interact with this object to do all of the encrypting, decrypting, analyzing, etc. The _self.shortcuts_ property contains a dictionary that holds shortcuts or abbreviations for actions that we want to take place. We'll get to how these are used and why they are important later, but for now know that we can use "d" instead of typing "display" in our solver. Classes based on the _CipherSolver_ class can add to this dictionary to add their own shortcuts. The _CipherSolver.display_ method simply prints out the contents of the _Cipher.text_. The other classes can override the display method to display in a different way that works better for their specific cipher types.

The _CipherSolver.solve_ method is the most important part of the _CipherSolver_ class because it essentially is the entire interactive solving mechanism. It is a bit complex and lengthy so we are also going to break it into chunks.

Before we get into that, why don't we go over how the solver will work so you have a basis for what the code is doing. The solver will perform actions that the user enters at its prompt. In the example below, lines that start with ">" are the prompt where the user enters the actions. Once the user presses enter, the solver will read through the action and arguments that the user has typed in and find the appropriate method in the solver class or the cipher class. Lets look at an example using the "sandbox.py":

Contents of sandbox.py

```python
from cipher import CipherSolver
solver = CipherSolver()
solver.cipher.text = 'This is a test of the emergency broadcast system.'
solver.solve()
```

```
python sandbox.py
> display

This is a test of the emergency broadcast system.

> d

This is a test of the emergency broadcast system.

>
```

When we execute "sandbox.py" we immediately get a ">" prompt to work with. In the example, the user typed "display" which prints out the contents of the _Cipher.text_. Notice that the user then types "d" and presses enter which gives the same results. This comes from the _CipherSolver.shortcuts_ property that we talked about earlier. It is just an easy way to execute frequently used actions.

Alright, so now back to the _CipherSolver.solve_ method. The solve method is just a giant loop. It continues to prompt the user for more actions until you kill it (Ctrl+C).

```python
  def solve(self):
    while True:
      try:
        raw_line = raw_input(self.prompt + " ")
        line = re.findall('\'[^\']*\'|\"[^\"]*\"|S+', raw_line)
        line = [item[1:-1] if item[0] in '\'"' else item for item in line]
```

Lines 33 takes the users input and runs it through a regular expression to break it apart into a list by splitting it at the spaces. If you want an argument that has spaces, just surround it in quotes. Line 34 just removes any beginning and ending quotes that may have been surrounding the arguments. Lets execute those lines by themselves in python so you can see line for line what it is doing.

```
>>> import re
>>> raw_line = raw_input("> ")
> command argument "quoted argument" "single quoted argument"
>>> line = re.findall(''[^']*'|"[^"]*"|S+', raw_line)
>>> print line
['command', 'argument', '"quoted argument"', '"single quoted argument"']
>>> line = [item[1:-1] if item[0] in ''"' else item for item in line]
>>> print line
['command', 'argument', 'quoted argument', 'single quoted argument']
```

So in the end, line will be a list of all the input split into little chunks that we can use. The first item in the list will always be the action or command that will be executed. The rest of the items will be arguments passed into the action methods that are called.

```python
        cmd = line.pop(0)
        attr = None
        if cmd in self.shortcuts:
          cmd = self.shortcuts[cmd]
```

Here we pop off the first item in the list and set it to the cmd variable. We then check to see if the cmd is in the _CipherSolver.shortcuts_ dictionary. If it is, we use that to lookup which real command will be executed (ie: "d" will change to "display"). Now that we know what command we need to run we go on:

```python
        if hasattr(self, cmd):
          attr = getattr(self, cmd)
        elif hasattr(self.cipher, cmd):
          attr = getattr(self.cipher, cmd)
```

Python has a great function that will let you know if an object has an attribute or method with a specific name. We use that to see if a method or attribute with name of the entered command exists in the _CipherSolver_ class first. If not found, we check in the attached _Cipher_ class. If it is found in one of these places, then we use the _getattr_ method to get a variable that represents the found method or attribute.

```python
        if attr  None:
          if type(attr) == types.MethodType:
            retvalue = attr(*line)
          else:
            retvalue = attr
          if retvalue != None:
            print retvalue
```

If the method or attribute was found, then the _attr_ variable will point it. If it is not found, then _attr_ will be _None_. If _attr_ is a method, we execute that method and pass in the rest of the user inputted contents from earlier. Python allows you to pass in a list to a method as arguments to that method if you place an asterisk in front of a list variable. For example, if we input "command argument1 argument2" then the following method would be called: command(argument1,argument2). If the _attr_ variable is pointing to an attribute then we just display the contents of that attribute. That means if we want to see what is in the _Cipher.text_ property we can just type "text" in the solver.

The rest of the code in "cipher.py" is just error trapping and displaying. We are almost to where we can do some actual solving. Just a little more to go. For now, lets create a new file called "shared.py" that will contain any shared functions that we want available for all our solver classes to use. Right now we'll only have one function in there but we'll add more as we progress.

Contents of shared.py

```python
#Breaks the text into lines with a maxium length of maxlen
def breaklines(text, maxlen):
  if maxlen <= 0:
      return text
  textlen = len(text)
  pos = 0
  output = ""
  while pos < textlen:
    chunk = text[pos:pos+maxlen]
    if (pos+maxlen+1 <= textlen) and (text[pos+maxlen+1]  ""):
      tpos = chunk.rfind(" ")
      if tpos  -1:
        chunk = text[pos:pos+tpos]

    output += chunk + "n"
    pos += len(chunk)
    if (pos < textlen) and (text[pos] == " "):
      pos += 1
  return output.strip("n ")
```

The _breaklines_ functions will break apart text into lines based on the maximum length passed into the function. We'll use this a lot when displaying text to the screen which may be too long to display normally. Now, we are finally get to create the _AristocratSolver_ class!

#### The AristocratSolver Class

Just like the we did before with "cipher.py" we will be adding a few import statements to the top of "aristocrat.py".

Contents of aristocrat.py (Place at the top of the file on line 4)

```python
import shared
```

Previously, we did not give the _Aristocrat_ class a good way of changing the plaintext and ciphertext keys. Lets add another method to the _Aristocrat_ class at the bottom of the file.

Contents of aristocrat.py (Place at the bottom of the file within the Aristocrat class)

```python
  def set(self, ct, pt = "-"):
    ct = ct.upper()
    pt = pt.lower()
    if pt == "-":
      pt = "-" * len(ct)
    for index in range(len(ct)):
      ctindex = self.ctkey.index(ct[index])
      self.ptkey = self.ptkey[:ctindex] + pt[index] + self.ptkey[ctindex+1:]
```

This function can take any number of ciphertext characters and sets the plaintext key values to the values passed in. For example: set("ABC","def") would change the plaintext key for "A" to "d", "B" to "e", and "C" to "f". So the ciphertext key would be "ABC" while the plaintext key would be "def". This allows us to pass in full words like "GJEG" could be "that". This will speed up solving a lot because you don't have to pick individual characters if you know a whole word.

Contents of aristocrat.py (Place at the bottom of the file below the Aristocrat class)

```python
class AristocratSolver(CipherSolver):
  def __init__(self):
    CipherSolver.__init__(self)
    self.cipher = Aristocrat()
    self.maxlinelen = 70
    self.shortcuts['s'] = "set"

  def display(self):
    data = ""
    ct = shared.breaklines(Cipher.encrypt(self.cipher), self.maxlinelen).split("n")
    pt = shared.breaklines(self.cipher.decrypt(), self.maxlinelen).split("n")

    for index in range(len(ct)):
      data += ct[index] + "n"
      data += pt[index] + "nn"
    print data.strip("n")

  def set(self, ct, pt = "-"):
    self.cipher.set(ct,pt)
    self.display()

  def __init__(self):
    CipherSolver.__init__(self)
    self.cipher = Aristocrat()
    self.maxlinelen = 70
    self.shortcuts['s'] = "set"
```

The initialization function calls the _CipherSolver_'s initialization function so we inherit all its properties. We then override the _CipherSolver.cipher_ property with a new _Aristocrat_ instance. We add a new shortcut to the _CipherSolver.shortcuts_ list for the "set" command. So we can just type "s" to speed up solving.

```python
  def display(self):
    data = ""
    ct = shared.breaklines(Cipher.encrypt(self.cipher), self.maxlinelen).split("n")
    pt = shared.breaklines(self.cipher.decrypt(), self.maxlinelen).split("n")

    for index in range(len(ct)):
      data += ct[index] + "n"
      data += pt[index] + "nn"
    print data.strip("n")
```

We override the _CipherSolver.display_ method with something more useful for Aristocrats. The _AristocratSolver.display_ method will display the ciphertext on one line and the decrypted plaintext below it. It will also break the lines up based on the _Aristocrat.maxlinelen_ property.

```python
  def set(self, ct, pt = "-"):
    self.cipher.set(ct,pt)
    self.display()
```

The last piece of the _AristocratSolver_ class is the set method. We want to create another set method because when solving we want to display the changes every time we set a new plaintext key value. Finally, lets see this thing in action! Change "sandbox.py" to the following and then execute it:

Contents of sandbox.py

```python
from aristocrat import AristocratSolver
solver = AristocratSolver()
solver.cipher.text = 'GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.'
solver.solve()
```

```
python sandbox.py
> d

GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.
---- -- - ---- -- --- --------- --------- ------.

> s VNVITVMXB emergency

GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.
---- -- - -e-- -- --e emergency -r---c--- -y--em.

>
```

Here we typed "d" to display the current state of the con. Then we typed "s VNVITVMXB emergency". We could have typed "s V e", "s N m" etc. but typing it all at once saves us a lot of time. As you can see, we used the shortcuts instead of typing out "display" and "set". We can add as many shortcuts as we want. Each solver class can have it's own shortcuts that are tailored to the specific solving needs of that class.

We have not created a fully working interactive cryptogram solver for Aristocrats! We could solve the entire con with this right now!

```
GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.
this is a test of the emergency broadcast system.
```

In [Part III](/2009/09/22/creating-an-interactive-cryptogram-solver-part-iii/) we will extend our existing classes by adding more functionality like frequency counting and key displaying to help us in our solving efforts. We'll also add support for Patristocrats!

#### Complete Source Code

You can download the complete source code to Part II at: [Creating an interactive cryptogram solver (Part II - Source)](http://www.codepenguin.com/2009/09/20/creating-an-interactive-cryptogram-solver-part-ii/solver_part_2/).

#### Related Posts:

[Creating an interactive cryptogram solver (Introduction)](/2009/09/17/creating-an-interactive-cryptogram-solver-introduction/)  
[Creating an interactive cryptogram solver (Part I)](/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/)  
[Creating an interactive cryptogram solver (Part III)](/2009/09/22/creating-an-interactive-cryptogram-solver-part-iii/)