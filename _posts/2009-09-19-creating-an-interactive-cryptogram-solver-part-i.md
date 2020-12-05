---
layout: post
title: Creating an interactive cryptogram solver (Part I)
date: 2009-09-19 16:04:11.000000000 -04:00
tags:
- ACA
- aristocra
- cryptography
- cryptograms
- programming
- python
- solver
permalink: "/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/"
---
![Execute Interactive Solver]({{ site.baseurl }}/assets/2009/09/Solver.png "Execute Interactive Solver")

In this post we'll first analyze what we are trying to accomplish and then begin to create an extensible framework that will allow us to adapt our solver to many different kinds of ciphers.

Building the framework
----------------------

Our main goal is to create an extensible interactive solver, so lets break down the similarities that all ciphers have.  You've got to think really generic here. All ciphers have the following similarities:

1.  They manipulate some kind of text.
2.  They have an encryption algorithm.
3.  They have a decryption algorithm.

With these three building blocks we can start creating our base framework.  Lets create a brand new folder that will contain all our source code and files that we will use.  I'm going name my folder "CryptogramSolver".  Whenever we create new files or want to execute anything, it will be done from this location.

<!--more-->
<a name="more" />

#### The Cipher Class

Start off by creating a brand new file called "cipher.py".  This file will contain our base class called "Cipher".  Since all ciphers will contain the building blocks from above, all our cipher classes will inherit from this base Cipher class.

Lets start off by creating our base Cipher class with the basic criteria from above.

Contents of cipher.py

```python
class Cipher:
  def __init__(self):
    self.text = ""

  def decrypt(self, text=""):
    if text == "":
      return self.text
    else:
      return text

  def encrypt(self, text=""):
    if text == "":
      return self.text
    else:
      return text
```

The above code declares our Cipher class and sets the _Cipher.text_ property to blank.  The _text_ property will contain whatever text we would like to encrypt or decrypt.  If we want to encrypt the _Cipher.text_ property we just call the _Cipher.encrypt()_ method.  The _Cipher.encrypt()_ and _Cipher.decrypt()_ methods will return a string that contains the encrypted or decrypted text.  These methods will not modify the value that is in _Cipher.text_.  This allows us to encrypt or decrypt the text many different times with different settings (keys, periods, etc.) without messing up our original text. Also note that we have an optional "text" argument for the _Cipher.encrypt()_ and _Cipher.decrypt()_. This allows us to encrypt or decrypt text that is not currently attached to our _Cipher_ object. We'll discuss how this is useful in a later post.

Lets create another file that will be our little playground or sandbox if you will called "sandbox.py". While we setup our framework we'll use the sandbox to test our code and try out any new features we add. The "sandbox.py" file should contain the following:

Contents of sandbox.py

```python
from cipher import Cipher
con = Cipher()
con.text = 'This is a test of the emergency broadcast system.'
ciphertext = con.encrypt()
print "Ciphertext: ", ciphertext
print "Plaintext: ", con.decrypt(ciphertext)
```

Here we import the Cipher class from "cipher.py" and then create a new instance of our Cipher class and call it "con". We then set the _Cipher.text_ property and the display the returned value of _Cipher.encrypt()_. Lets see what we get when we execute our sandbox.py:

python sandbox.py

```
Ciphertext:  This is a test of the emergency broadcast system.
Plaintext:  This is a test of the emergency broadcast system.
```

So _Cipher.encrypt()_and _Aristocrat.decrypt()_ just return the _Cipher.text_ property? That doesn't seem very useful at all.  Well, since the Cipher class is just our base class, it doesn't have any knowledge about what kind of cipher we are using or how to interact with it.  Any new classes that we create will override the Cipher.encrypt() and Cipher.decrypt() methods with their own methods that will perform the real encryption and decryption.

#### The Aristocrat Class

Alright! Now we can finally get to the fun stuff. Lets build our Aristocrat class. Start by creating a new file called "aristocrat.py" that contains the following:

Contents of aristocrat.py

```python
import string
from cipher import Cipher

class Aristocrat(Cipher):
  def __init__(self):
    Cipher.__init__(self)
    self.ctkey = string.ascii_uppercase)
    self.ptkey = "-" * 26

  def decrypt(self, text = ""):
    text = Cipher.decrypt(self, text)
    return self.process(self.ctkey, self.ptkey, text.upper())

  def encrypt(self, text = ""):
    text = Cipher.encrypt(self, text)
    return self.process(self.ptkey, self.ctkey, text.lower())

  def process(self, key1, key2, text):
    output = ""
    for char in text:
      if char in key1:
        output += key2[key1.index(char)]
      elif char.lower() in string.ascii_lowercase:
        output += "-"
      else:
        output += char
    return output
```

There isn't a whole lot to our Aristocrat class yet but lets go over what we've got so far.

```python
  def __init__(self):
    Cipher.__init__(self)
    self.ctkey = string.ascii_uppercase
    self.ptkey = "-" * 26
```

Here we are calling the _Cipher.\_\_init\_\_(self)_ so that our _Aristocrat_ class will have all the same properties as the _Cipher_ class. So we can use _Aristocrat.text_ even thought it isn't specifically in our class. That's how class inheritance works so we won't go into detail here as there are tons of other resources that could explain it better. Our class needs to have two different kinds of keys. We have _Aristocrat.ctkey_ that contains the ciphertext key and _Aristocrat.ptkey_ contains the plaintext key. Since we are initializing our keys, the _Aristocrat.ctkey_ will start out with A-Z and the _Aristocrat.ptkey_ will start out with twenty-six dashes. We set it up this way just as our primary goal for this class is to solve Aristocrats which means filling in the _Aristocrat.ptkey_ with the correct plaintext characters.

If you wanted to decrypt a ciphertext letter, you would find what position it is in _Aristocrat.ctkey_ and then use the character at the same position in _Aristocrat.ptkey_ as the plaintext character. That is exactly what our decrypt method will do and encrypt is just the opposite.

```
Example:
 ctkey = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
 ptkey = 'zyxwvutsrqponmlkjihgfedcba'
 ctkey[13] = 'N'
 ptkey[13] = 'm'
```

Now lets go over the _Aristocrat.encrypt_ and _Aristocrat.decrypt_ methods.

```python
  def decrypt(self, text = ""):
    text = Cipher.decrypt(self, text)
    return self.process(self.ctkey, self.ptkey, text.upper())

  def encrypt(self, text = ""):
    text = Cipher.encrypt(self, text)
    return self.process(self.ptkey, self.ctkey, text.lower())
```

The _Aristocrat.encrypt_ and _Aristocrat.decrypt_ methods both call the _Cipher.encrypt_ method to grab the text that they should use for encrypting or decrypting. We do this so that we execute all the same code that _Cipher.encrypt_ would because if the text argument is blank, we want to use the _Cipher.text_ property instead. Both of these methods call the same _Aristocrat.process_ procedure with one small difference. The _Aristocrat.encrypt_ passes in the _Aristocrat.ptkey_ first and then _Aristocrat.ctkey_ while the decrypt method does the opposite. This allows us to use the same method to encrypt and decrypt our text by simply swapping the keys.

Now comes the real meat of the class! The _Aristocrat.process_ method does all the real work.

```python
  def process(self, key1, key2, text):
    output = ""
    for char in text:
      if char in key1:
        output += key2[key1.index(char)]
      elif char.lower() in string.ascii_lowercase:
        output += "-"
      else:
        output += char
    return output
```

The _Aristocrat.process_ method goes through each character in the text argument and sees if it can find that character in key1. If it is found, it grabs the character in the same position in key2. If not found, it will display a dash if the character is A-Z or just print the character if it is anything else (ie: punctuation, spaces, etc.).

That is pretty much it. Lets change our sandbox.py file so we can use our new class.

Contents of sandbox.py

```python
from aristocrat import Aristocrat
con = Aristocrat()
con.text = 'This is a test of the emergency broadcast system.'
con.ptkey = 'zyxwvutsrqponmlkjihgfedcba'
ciphertext = con.encrypt()
print "Ciphertext: ", ciphertext
print "Plaintext: ", con.decrypt(ciphertext)
```

Instead of using the _Cipher_ class, we'll use the _Aristocrat_ class. We set the _Aristocrat.ptkey_ to the value that we want and then display the encrypted text. Lets execute sandbox.py and see how things have changed:

```
python sandbox.py
Ciphertext:  GSRH RH Z GVHG LU GSV VNVITVMXB YILZWXZHG HBHGVN.
Plaintext:  this is a test of the emergency broadcast system.
```

Alright! Now we have a class that can encrypt and decrypt any Aristocrat cons! We can extend the _Cipher_ class to add any extra functionality that we want all our cipher classes to have access to. In [Part II](/2009/09/20/creating-an-interactive-cryptogram-solver-part-ii/) we'll finish up our framework by creating the generic _CipherSolver_ class and the _AristocratSolver_ class.

#### Related Posts:

[Creating an interactive cryptogram solver (Introduction)](/2009/09/17/creating-an-interactive-cryptogram-solver-introduction/)  
[Creating an interactive cryptogram solver (Part II)](/2009/09/20/creating-an-interactive-cryptogram-solver-part-ii/)