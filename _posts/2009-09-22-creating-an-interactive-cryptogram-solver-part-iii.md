---
layout: post
title: Creating an interactive cryptogram solver (Part III)
date: 2009-09-22 20:38:28.000000000 -04:00
tags:
- ACA
- aristocra
- cryptography
- cryptograms
- programming
- python
- solver
permalink: "/2009/09/22/creating-an-interactive-cryptogram-solver-part-iii/"
---
![Execute Interactive Solver]({{ site.baseurl }}/assets/2009/09/Solver.png "Execute Interactive Solver")

In Part I and Part II of this series we created the framework for our interactive solver and finally had a working _AristocratSolver_ class. In this part, we will enhance our existing framework by adding some commonly used functions, add frequency counting of characters and character sequences to the _AristocratSolver_ class, add the ability to display the current plaintext and ciphertext keys to the _AristocratSolver_ class and then finally create a _PatristocratSolver_ class that reuses all our work in the _AristocratSolver_ class.  

<!--more-->
<a name="more" />

#### Extending the Cipher Class

In the previous parts of this series, the _Cipher_ class had two methods, encrypt and decrypt, that basically just returned the _Cipher.text_ property. A problem that we will run into is that the _Cipher.text_ property could contain characters that are not compatible with the cipher class that we are using. We'll want to strip out any invalid characters before we had the text off to the encryption and decryption routines of the actual cipher class. First, lets import the shared class at the top of cipher.py so we can use it:

Contents of cipher.py (Place at the top of the file on line 3)

```python
import shared
```

Now lets modify the _Cipher.\_\_init\_\_ method as follows:

Contents of cipher.py (Replace the existing Cipher.\_\_init\_\_ method)

```python
  def __init__(self):
    self.text = ""
    self.decrypt_filter = lambda char: True
    self.encrypt_filter = lambda char: True
```

Here we've added self.decrypt\_filter and self.encrypt\_filter variables. These two variables will actually hold the functions that will get called for each character in _Cipher.text_ and will return True or False depending on if the character is valid or not. We are using a really cool feature of python here called "lambda". Lambda creates a single expression function. They are in the format:

lambda arguments: expression

We'll see a better use of lambdas in the _Aristocrat_ class in a moment. For now, the _Cipher.decrypt\_filter_ and _Cipher.encrypt\_filter_ just pass in each character and always return True. So the text will go unchanged because all characters are considered valid. Now lets add a method that will handle all the filtering of the text.

Contents of cipher.py (Insert after the Cipher.\_\_init\_\_ method)

```python
  def get_text(self, text="", filter_func = None):
    if text == "":
      text = self.text
    if filter_func == None:
      return text
    else:
      filtered_text = ""
      for char in text:
        if filter_func(char):
          filtered_text += char
      return filtered_text
```

The _Cipher.get\_text_ method will use the _Cipher.text_ property unless the text argument is passed. It then walks through the text and calls the filter\_func for each character in the text. If filter\_func returns True, the character is added to the return value. This is the perfect place to strip off line feeds or any other undesired character. The filter\_func can be any function that has a character argument and returns True or False. Lets modify our _Cipher.encrypt_ and _Cipher.decrypt_ methods to take advantage of this new _Cipher.get\_text_ method.

Contents of cipher.py (Replace the existing Cipher methods)

```python
  def decrypt(self, text=""):
    return self.get_text(text, self.decrypt_filter)

  def encrypt(self, text=""):
    return self.get_text(text, self.encrypt_filter)
```

The _Cipher.encrypt_ and _Cipher.decrypt_ methods are much simpler now because they just have to call the _Cipher.get\_text_ method with the appropriate filter. Alright, the _Cipher_ class is now ready for other classes to utilize its new functionality! Now lets modify the _CipherSolver_ to contain a few commonly used functions.

#### The CipherSolver Class

Contents of cipher.py (Replace the existing CipherSolver.\_\_init\_\_ method)

```python
  def __init__(self):
    self.cipher = Cipher()
    self.prompt = ">"
    self.maxlinelen = 70
    self.shortcuts = {"d":"display"}
```

Here we've changed the _CipherSolver.\_\_init\_\__ method by adding "self.maxlinelen = 70". We want to add this here because we will be using the _shared.breaklines_ function and will need to know what the maximum line length will be. This means that we can remove this line from the _Aristocrat_ class if we want to. Now lets add a new method below the _CipherSolver.display_ method.

Contents of cipher.py (Replace the existing CipherSolver.\_\_init\_\_ method)

```python
  def print_counts(self,counts):
    retvalue = ""
    if len(counts) == 0:
      retvalue = "None"
    else:
      for item,count in counts:
        retvalue += "%s:%s, " % (item,count)
    print shared.breaklines(retvalue.strip(", "), self.maxlinelen)
```

A lot of our cipher classes are going to be calculating and counting things like frequency counts, digraph and trigraph counts, etc. So the _CipherSolver.print\_counts_ method neatly display those counts on the screen. All our cipher classes will be able to use this method and uniformly display their counts as needed

#### Shared functions

Before we jump into the _Aristocrat_ class, lets add a few shared functions to our "shared.py" file. Add the imports to the top of the file and then add the two functions. I like to keep things in alphabetical order but that is totally up to you.

Contents of shared.py (Place at the top of the file)

```python
import string
from operator import itemgetter

Contents of shared.py (ADD at the top of the file)

#Breaks the text into blocks of length blocklen
def breakblocks(text,blocklen):
  output = ""
  currlen = 0
  for index in range(len(text)):
    output += text[index]
    currlen += 1
    if currlen == blocklen:
      output += " "
      currlen = 0
  return output
```

The _breakblocks_ will break a long line into chunks based on the blocklen parameter. This will be used in the _PatristocratSolver_ class when we want to display things in 5 character blocks. Lets use the python interpreter and see how to use it:

```
python
>>> import shared
>>> line = 'GSRHRHZGVHGLUGSVVNVITVMXBYILZWXZHGHBHGVN'
>>> shared.breakblocks(line,5)
'GSRHR HZGVH GLUGS VVNVI TVMXB YILZW XZHGH BHGVN '
>>>
```

```python
#Calculates how many times sequences of characters appears in the text
def calc_graphs(items, length, sorted = True, keepsingles = False, valid_chars = string.ascii_letters):
  if type(items) != list:
    items = [items]
  counts = {}
  for text in items:
    temp = ""
    for index in range(len(text)):
      if text[index] in valid_chars:
        temp = temp + text[index]
    text = temp
    for index in range(len(text)-(length-1)):
      graph = text[index:index+length]
      if not graph in counts:
        counts[graph] = 0
      counts[graph] += 1
  counts = counts.items()
  if not keepsingles:
    counts = [(k,v) for k,v in counts if v > 1]
  if sorted:
    counts.sort(key=itemgetter(1), reverse=True)
  return counts
```

The _calc\_graphs_ function takes a string or list of strings and calculates all the repeated sequences of characters that are of the length specified. If we want to throw away any that only appear once, we can pass in False for the keepsingles parameter. The valid\_chars parameter is a string containing all the valid characters that will be counted. Lets look at an example of its usage:

```
python
>>> import shared
>>> line = 'GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.'
>>> shared.calc_graphs(line,1)
\[('G', 6), ('H', 6), ('V', 6), ('Z', 3), ('B', 2), ('I', 2), ('L', 2), ('N', 2), ('S', 2), ('R', 2), ('X', 2)\]
>>>
```

If we want to see the digraphs we can pass in 2 as the length or 3 for trigraphs. By default the result is sorted descending from largest to smallest count.

#### The Aristocrat Class

Alright, now lets put all this new functionality into the _Aristocrat_ class. Lets override the _Cipher.encrypt\_filter_ and _Cipher.decrypt\_filter_ properties in the _Aristocrat.\_\_init\_\__ method.

Contents of aristocrat.py (Replace the existing Aristocrat.\_\_init\_\_ method)

```python
  def __init__(self):
    Cipher.__init__(self)
    self.ctkey = string.ascii_uppercase
    self.ptkey = "-" * 26
    self.decrypt_filter = lambda char: char in (string.ascii_letters + string.punctuation + " ")
    self.encrypt_filter = self.decrypt_filter
```

Here we are setting the _decrypt\_filter_ to a new lambda function. This function checks to see if the character passed is a letter, punctuation or a space. We want the _encrypt\_filter_ to be the same so we are just assigning it to the value of the _decrypt\_filter_. Now, whenever we need to get the text in the _Aristocrat_ class we will strip out any line feeds, digits or other invalid characters automatically.

#### The AristocratSolver Class

Now lets add our new frequency counting functionality to the _AristocratSolver_ class.

Contents of aristocrat.py (Replace the existing AristocratSolver.\_\_init\_\_ method)

```python
  def __init__(self):
    CipherSolver.__init__(self)
    self.cipher = Aristocrat()
    self.shortcuts['f'] = 'frequency_list'
    self.shortcuts['k'] = 'keys'
    self.shortcuts['s'] = "set"
```

Here we'll add two new shortcuts to the _Cipher.shortcuts_ list in the _AristocratSolver.\_\_init\_\__ method for the _frequency\_list_ method and the keys method. So whenever we want to get frequency counts, we just have to type "f". If you want digraphs just type "f 2" into the solver. If we want to display the keys we can just type "k". Notice we've removed the "self.maxlinelen = 70" line from the _AristocratSolver.\_\_init\_\__ method. Since the _CipherSolver.\_\_init\_\__ method already declares this, we don't need to do this. If we want to change the _maxlinelen_ globally, we can just change it in _CipherSolver_.

The last thing we need to do is add the actually _frequency\_list_ method to the _AristocratSolver_ class below the _display_ method.

Contents of aristocrat.py (Insert after the AristocratSolver.display method)

```python
  def frequency_list(self, length = 1, text = ""):
    text = Cipher.encrypt(self.cipher, text)
    self.print_counts(shared.calc_graphs(text.split(" "), int(length)))
```

This method just gets the _Cipher.text_ property and uses the _shared.calc\_graphs_ and _print\_counts_ functions to calculate and display the frequency counts. Notice that the text that we are passing into the _shared.calc\_graphs_ function is actually being split into chunks at the spaces. This makes sure that we only see repeated patterns in the actual words themselves and not just mashed together by ignoring the spaces

Lets see this new method in action! Lets fire up our "sandbox.py" in python:

```
python sandbox.py
> d

GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.
---- -- - ---- -- --- --------- --------- ------.

> f 1

G:6, H:6, V:6, Z:3, B:2, I:2, L:2, N:2, S:2, R:2, X:2

> f 2

HG:3, GV:2, GS:2, VN:2, RH:2

>
```

Now lets add the displaying of plaintext and ciphertext keys to the _AristocratSolver_ class.

Contents of aristocrat.py (Insert after the AristocratSolver.frequency\_list method)

```python
  def keys(self):
    print "ct:", self.cipher.ctkey, "npt:", self.cipher.ptkey, "n"
    values = zip(self.cipher.ptkey, self.cipher.ctkey)
    values.sort()
    ptval=""
    ctval=""
    for pt,ct in values:
      ptval += pt
      ctval += ct
    print "pt:",ptval, "nct:", ctval
```

The key function displays the ciphertext and plaintext keys, first in alphabetical ciphertext order and then alphabetical plaintext order. Lets see what it looks like when executed:

```
python sandbox.py
> s abcdefghijklmnopqrstuvwxyz zyxwvutsrqponmlkjihgfedcba

GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.
this is a test of the emergency broadcast system.

> k

ct: ABCDEFGHIJKLMNOPQRSTUVWXYZ
pt: zyxwvutsrqponmlkjihgfedcba

pt: abcdefghijklmnopqrstuvwxyz
ct: ZYXWVUTSRQPONMLKJIHGFEDCBA

>
```

There we go! We've extended our _AristocratSolver_ class so that it can display frequency counts and keys. We've modified our _Cipher_ and _CipherSolver_ classes to make them more extendable for other cipher classes. The last thing we need to do is whip up the _Patristocrat_ and _PatristocratSolver_ classes.

#### The Patristocrat Class

Since Patristocrats are basically just Aristocrats without the word spacings, we can re-use a ton of the _Aristocrat_ class functionality and only override the different parts. Lets create a new file called "patristocrat.py" and put in the following code:

Contents of patristocrat.py

```python
import string
import shared
from cipher import Cipher
from aristocrat import Aristocrat, AristocratSolver

class Patristocrat(Aristocrat):
  def __init__(self):
    Aristocrat.__init__(self)
    self.decrypt_filter = lambda char: char in string.ascii_letters
    self.encrypt_filter = self.decrypt_filter
```

The _Patristocrat_ class is quite small because the _Aristocrat_ class already does everything. Notice we are inheriting from the _Aristocrat_ class and calling the _Aristocrat.\_\_init\_\__ method. The _Aristocrat_ class allowed letters, punctuation, and spaces in both its encrypted and decrypted text. For Patristocrats, we really only want letters. So we change the _decrypt\_filter_ and _encrypt\_filter_ to only return True if the character is a letter (A-Z, a-z).

#### The PatristocratSolver class

The _PatristocratSolver_ class has a little more code than its cipher.

Contents of patristocrat.py (Place after the Patristocrat class)

```python
class PatristocratSolver(AristocratSolver):
  def __init__(self):
    AristocratSolver.__init__(self)
    self.cipher = Patristocrat()

  def display(self):
    data = ""
    ct = shared.breakblocks(Cipher.encrypt(self.cipher), 5)
    ct = shared.breaklines(ct, self.maxlinelen).split("n")
    pt = shared.breakblocks(self.cipher.decrypt(), 5)
    pt = shared.breaklines(pt, self.maxlinelen).split("n")

    for index in range(len(ct)):
      data += ct[index] + "n"
      data += pt[index] + "nn"
    print data.strip("n")

  def frequency_list(self, length = 1, text = ""):
    text = Cipher.encrypt(self.cipher, text)
    self.print_counts(shared.calc_graphs(text, int(length)))
```

Lets go through each of the three methods in the class

```python
  def __init__(self):
    AristocratSolver.__init__(self)
    self.cipher = Patristocrat()
```

First we have the _PatristocratSolver.\_\_init\_\__ method. This calls the _AristocratSolver.\_\_init\_\__ method so it can inherit all its functionality. We then change the _self.cipher_ to be a new instance of the _Patristocrat_ class.

```python
  def display(self):
    data = ""
    ct = shared.breakblocks(Cipher.encrypt(self.cipher), 5)
    ct = shared.breaklines(ct, self.maxlinelen).split("n")
    pt = shared.breakblocks(self.cipher.decrypt(), 5)
    pt = shared.breaklines(pt, self.maxlinelen).split("n")

    for index in range(len(ct)):
      data += ct[index] + "n"
      data += pt[index] + "nn"
    print data.strip("n")
```

The _display_ method gets the text from the _Cipher.encrypt_ method and then breaks it into five character blocks. We use the _shared.breaklines_ method again to break the lines if they are too long. We also break the decrypted text into blocks and then display the ciphertext and plaintext together.

```python
  def frequency_list(self, length = 1, text = ""):
    text = Cipher.encrypt(self.cipher, text)
    self.print_counts(shared.calc_graphs(text, int(length), False))
```

Finally, we have the _frequency\_list_ method. The only thing that is different from the _AristocratSolver.frequency\_list_ is that we are not splitting the text at the spaces or line breaks.

That is it for the _Patristocrat_ and _PatristocratSolver_ classes. We saved a lot of work by inheriting from the _Aristocrat_ classes. Now lets modify "sandbox.py" and take her for a spin.

Contents of sandbox.py (Replace existing contents)

```python
from patristocrat import PatristocratSolver
solver = PatristocratSolver()
solver.cipher.text = 'GSRHRHZGVH GLUGSVVNVITVMXBYIL ZWXZHGHBHGVN'
solver.solve()
```

So we just set everything to use the _Patristocrat_ classes instead. I've thrown a little ciphertext in there with spaces so you can see that they will be stripped out by the filters. Lets see how this looks in python:

```
\> d

GSRHR HZGVH GLUGS VVNVI TVMXB YILZW XZHGH BHGVN
----- ----- ----- ----- ----- ----- ----- -----

> f

G:6, H:6, V:6, Z:3, B:2, I:2, L:2, N:2, S:2, R:2, X:2

> s VNVITVMXB emergency

GSRHR HZGVH GLUGS VVNVI TVMXB YILZW XZHGH BHGVN
----- ---e- ----- eemer gency -r--- c---- y--em

> k

ct: ABCDEFGHIJKLMNOPQRSTUVWXYZ
pt: -y------r---nm-----g-e-c--

pt: -------------------cegmnry
ct: ACDEFGHJKLOPQRSUWYZXVTNMIB

>
```

It works! You can still use all the _AristocratSolver_ methods like _keys_. Our framework has been completed and we've seen how to create two different cipher classes and interactive solvers. They can be extended to fit any solving style or cipher type. In Part IV of this series we will bring all the code together into a complete interactive solver that allows you to select which solver class you would like to use. We will also add documentation functionality to the classes so that the solver can tell you all about the functions and parameters without us having to constantly look at the source code or memorize everything.

#### Complete Source Code

You can download the complete source code to Part III at: [Creating an interactive cryptogram solver (Part III – Source)](http://www.codepenguin.com/2009/09/22/creating-an-interactive-cryptogram-solver-part-iii/solver_part_3/).

#### Related Posts:

[Creating an interactive cryptogram solver (Introduction)](/2009/09/17/creating-an-interactive-cryptogram-solver-introduction/)  
[Creating an interactive cryptogram solver (Part I)](/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/)  
[Creating an interactive cryptogram solver (Part II)](/2009/09/20/creating-an-interactive-cryptogram-solver-part-ii/)  
[Creating an interactive cryptogram solver (Part IV)](/2009/10/31/creating-an-interactive-cryptogram-solver-part-iv/)