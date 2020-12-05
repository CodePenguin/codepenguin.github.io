---
layout: post
title: Creating an interactive cryptogram solver (Introduction)
date: 2009-09-17 21:41:49.000000000 -04:00
tags:
- ACA
- aristocrat
- cryptograms
- cryptography
- patristocrat
- programming
- python
- solver
permalink: "/2009/09/17/creating-an-interactive-cryptogram-solver-introduction/"
---
![Execute Interactive Solver]({{ site.baseurl }}/assets/2009/09/Solver.png "Execute Interactive Solver")

I've been a member of the [ACA (American Cryptogram Association)](http://www.cryptogram.org "American Cryptogram Association") for about a year now.  I started out solving Aristocrats and Patristocrats with pen and paper.  It was definitely a slow start as trial and error created a lot of eraser dust on my desk.  Being a programmer by trade, my brain instantly sees how I could speed up the process using computers and programming.  Now, I didn't want to ruin the sense of accomplishment that I got when I solved my first Aristocrat by hand by making the computer just do all the work for me.  It has taken much constraint for me not to write an automatic solver.  The happy medium I found was with computer assisted solving.  Let the computer do all the tedious manual labor and let my mind work on the actual solving process and techniques.

I started writing my own interactive solver about a month after I joined the ACA and I wanted to share my experience with others.  Hopefully, someone will find this information useful or it might inspire them to delve into cryptography or programming.

My prerequisites for an interactive solver are as follows:

1.  Has to be easily modified as needed when inspiration strikes.
2.  Has to be extendable so that it can be utilized for any kind of cipher.
3.  The programming language used has to be freely available and easy to install on most major operating systems.
4.  The programming language used has to be relatively easy to understand for new users.

Out of all of the languages that I have used over the years, Python scored really high with the above prerequisites.  Python comes pre-installed on most major operating systems (except windows) or is very easy to obtain and install.  Now this doesn't mean that you can't use a different programming language to create an interactive solver.  Any programming language will work just fine.  Whatever you are comfortable with is the best choice for you.

There are many resources for learning python so I won't be going into great detail over all the language features or syntax nor will this be a beginner's tutorial for python.  If you are in need of python learning materials check out the following:

*   [How to think like a computer scientist: Learning with Python](http://openbookproject.net/thinkCSpy/ "How to think like a computer scientist: Learning with Python")
*   [Dive Into Python](http://diveintopython.org/ "Dive Into Python")
*   [Mike Cowan's Learning to program in python for novice programmers and cipher enthusiasts](http://web.mac.com/mikejcowan/Ciphers/1._Introduction.html "Mike Cowan's Learning to program in python for novice programmers and cipher enthusiasts")

This is only the introduction but we'll jump right into the programming in [Part I](/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/).  Each Part will extend our solver with more functionality and hopefully we'll end up with a easy to use interactive solver!

Note: For now all the code will be for Python 2.6.  Most operating systems are still bundling this version so it is the most accessible for now.

#### Related Posts:

[Creating an interactive cryptogram solver (Part I)](/2009/09/19/creating-an-interactive-cryptogram-solver-part-i/)