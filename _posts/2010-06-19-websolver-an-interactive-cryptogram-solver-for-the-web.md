---
layout: post
title: 'WebSolver: An Interactive Cryptogram Solver for the Web!'
date: 2010-06-19 22:48:37.000000000 -04:00
tags:
- ACA
- aristocrat
- cryptograms
- cryptography
- patristocrat
- programming
- solver
permalink: "/2010/06/19/websolver-an-interactive-cryptogram-solver-for-the-web/"
---
> A great carpenter does not need to know how to make a hammer or saw in order to create a masterpiece.

[![Web Solver: Start]({{ site.baseurl }}/assets/2010/06/websolver_start.jpg "Web Solver: Start")](http://tools.codepenguin.com/websolver/)  
A while back I wrote a bunch of articles describing how to create your own [interactive cryptogram solver](/2009/09/17/creating-an-interactive-cryptogram-solver-introduction/) using python. I've used it a lot in my own personal solving and have had a few friends use it too but it definitely is not a tool for everyone. There are a lot of great tools out there for solving ciphers but many of them are very technical, require a special environment to run, or require programming knowledge and experience. Now, I don't want to down play the usefulness of these great tools or tell anyone that they shouldn't learn computer programming (Note: I think all computer users should know the very basics of programming logic as it is so useful for many things). That being said, there are many people out there who love a good challenge but either don't have the desire or the time to learn to program. Sometimes the greater skill is knowing how to use the tools properly and efficiently. A great carpenter does not need to know how to make a hammer or saw in order to create a masterpiece.

So for all you great carpenters (solvers) out there, I have ported my interactive cryptogram solver to the web! No downloads, no special environments (other than a modern web browser), and no programming skills needed. Just a clean area where you can do what you do best: solving.

<!--more-->
<a name="more" />

**Web Solver is available now at [http://tools.codepenguin.com/websolver/](http://tools.codepenguin.com/websolver/).**

At the moment, the Web Solver only has support for Aristocrats and Patristocrats. I will be adding a lot more as time progresses. I'm still learning so I'll add my techniques as I learn them. If you have any suggestions or different techniques that could be added, please leave a comment or contact me. I'm always open for suggestions. If you are familiar with my previous interactive solver, then the interface will be very similar. This time there is an integrated help menu available on the right side of the screen that explains all the commands. The interface is mostly keyboard driven as changes can be made more quickly with the keyboard than the mouse. This allows many different types of ciphers to be solved in a similar interface with commands that are specially designed for them.

Quick Overview
--------------

![Web Solver: Loading Screen]({{ site.baseurl }}/assets/2010/06/websolver_load.jpg "Web Solver: Loading Screen")  
Lets run through a quick session of using the Web Solver. I'm going to grab the A-2 cipher from the [sample issue of the Cryptogram](http://cryptogram.org/sample.html). When you first open the Web Solver for the first time, it should automatically show you the "Load" Screen. This screen can be accessed at any time by clicking the load button on the right hand side of the screen. The help button is also in that same area providing a handy command reference and usage guide. I'm going to paste/type in the A-2 cipher text into the text area on the loading screen and then click the "Start Solving!" button.

Note: As we go through a few examples of commands, they will be displayed in little terminal boxes in this article for display and usage purposes. Images would be too small and you wouldn't be able to copy and paste from them. So just be aware that the look will be simulated but pretty close to what the real Web Solver looks like.

For all the cipher types there are three main commands: _clear, display, and pattern_. _Clear_ just clears the screen in case you want to remove the clutter. _Display_ (shortcut: d) displays the current state of the cipher. This generally will display both the ciphertext and the current state of the plaintext. Since all cipher types are different, the display will vary depending on the type. For example: Aristocrats will have word spacings while Patristocrats will be broken into five letter groups. Lets check out what the display command looks like for our current con.

```
Aristocrat Solver loaded.
> d
OB ISZDPH *GQG EFBE KZE NZUZPJ SQQO ZE EQ EOFNN AKFA BQT YFP'A EKQTA
-- ------ *--- ---- --- ------ ---- -- -- ----- ---- --- ---'- -----

FA AKD YFA VZAKQTA JDAAZPJ F OQTAKITN QI KFZS.
-- --- --- ------- ------- - -------- -- ----.
>
```

Useful commands
---------------

Now that we can see our ciphertext, lets take a look some patterns. The _patterns_ command (shortcut: p) can be used to quickly look up a pattern and return any words that match it. This command is pretty powerful so definitely read the help on it for more information. Lets just look at a quick example. We'll take at a few examples from the con:

```
> p ISZDPH
389 matches found.
absurd acting action admire adults advice advise agency agents aliens almost amount
angels answer anyhow around asking author awhile backed backup barely basket beacon
beauty begins behalf behind beings belong betray beyond bishop blocks blonde bodies
boring bother bought brains breaks breast breath bridge bright brings broken budget
burden buried burned busted buster buying campus candle caring carpet casino castle
caught caused chairs change chapel charge chosen cipher claims client closed closer
closet column comedy coming counts county couple course cousin covers coward credit
crimes cruise crying custom dancer danger dating deacon demons denial deputy design
detail direct double doubts
290 words not shown.

> p OQTAKITN
5 matches found.
adoption handling partners patients ultimate

> p JDAAZPJ
1 match found.
getting
```

Notice that you can grab the words directly from the ciphertext. You can also search for number based patterns and use filters to narrow down the results. There are four different word lists currently available with varying amounts of words. You can switch to a different word list at any time. We'll use the _set_ (shortcut: t) command to set the plaintext letters for one of the pattern words that we just found.

```
> s JDAAZPJ getting
OB ISZDPH *GQG EFBE KZE NZUZPJ SQQO ZE EQ EOFNN AKFA BQT YFP'A EKQTA
-- --ien- *--- ---- -i- -i-ing ---- i- -- ----- t--t --- --n't ----t

FA AKD YFA VZAKQTA JDAAZPJ F OQTAKITN QI KFZS.
-t t-e --t -it---t getting - ---t---- -- --i-.
```

The _set_ command will except full words or single letters. You can even reset a letter by only giving a ciphertext letter. (Example: s X). Now that we have some letters we can narrow down one of our previous pattern searches that had 389 matches.

```
> p ISZDPH --ien-
3 matches found.
aliens client friend
```

That looks much better. These filters can be used to quickly narrow down the results. You can even use wild cards if you don't know exact lengths.

If you want to see the frequency chart, you can use the _frequency_ command (shortcut: f). It can even give you digraphs and trigraphs if you give it a length.

```
> f
A:12 Q:9 Z:8 F:8 E:7 K:7 T:5 O:4 P:4 N:4 B:3 I:3 S:3 D:3 J:3 G:2 Y:2 H:1 U:1 V:1

> f 2
AK:4 QT:4 FA:3 TA:3 ZE:2 ZP:2 PJ:2 KF:2 YF:2 KQ:2

> f 3
QTA:3 ZPJ:2 KQT:2
```

These are only a few of the commands that are available. The help system will give you details for everything with examples. If you have any ideas for new commands, just let me know. They can be easily added and then everyone can have access to new solving techniques.

Save and Continue
-----------------

Ever been solving a cipher and had to go do something else? Maybe your computer crashed? The Web Solver will automatically save your progress and allow you to come right back where you were as long as you're using the same computer. All the data is stored locally on your computer.

Command History
---------------

Web Solver will keep the history of the last 25 commands that you have used. You can go back through them by pressing up and down. This way you can save some typing if you want to go back to a previously used command.

Conclusion
----------

Web Solver is a solving tool. It won't do all the work for you but hopefully it will make things easier. **It is available now at [http://tools.codepenguin.com/websolver/](http://tools.codepenguin.com/websolver/).** Please, let me know what you think. Leave a comment or [contact me](/contact-me/) directly. I love to hear your ideas and suggestions. I will continue to improve Web Solver and add new features as time goes on.

Technical Details and Links
---------------------------

For those interested, I'd like to mention the different technologies and techniques that made this possible.

*   [jQuery](http://jquery.com/) - A powerful javascript library that makes cross-platform web development a whole lot easier. This is pretty much the core of the entire implementation.
*   [Taffy DB](http://taffydb.com/) - A JavaScript library that acts as an in memory data layer. This powers the CP List which is a small embedded word list. All the other word lists require an external database connection. CP List can be used without an internet connection.
*   [LazyLoad](http://wonko.com/post/lazyload-200-released) - A JavaScript library used to load files dynamically only when needed. This is used to load the word list functionality on the fly without taking up a lot of memory.
*   [PersistJS](http://pablotron.org/?cid=1557) - A JavaScript library that provides client-side persistent storage. This handles all the storage needs and works great in all modern browsers.
*   [How To Create A Sexy Vertical Sliding Panel Using jQuery And CSS3](http://spyrestudios.com/how-to-create-a-sexy-vertical-sliding-panel-using-jquery-and-css3/) - This tutorial helped me create the sliding Load and Help panels.