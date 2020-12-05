---
layout: post
title: 'Pattern Words: A Solver''s Staple'
date: 2010-06-11 22:08:49.000000000 -04:00
tags:
- ACA
- aristocrat
- cryptography
- cryptograms
- patristocrat
permalink: "/2010/06/11/pattern-words-a-solvers-staple/"
---
While working on the [Beginner's Guide to the ACA](/2010/06/06/beginners-guide-to-the-aca/), there was one technique that consistently showed up in all the research material: Pattern Words! At first I didn't really understand how great they were. For those who don't know about them or need a refresher, I'll discuss a few of the different pattern types that I have come across. Afterwards, I'll discuss my own Pattern Word Search Tool. Lets get started!

<!--more-->
<a name="more" />

Pattern Types
-------------

#### Fixed Length Patterns

Pattern words have one or more letters that are repeated. These repeated letters provide a clue to the word's identity. Start by taking a word and assigning numbers to each letter starting at the left with 1 . Use the same number for repeated letters. Now you can look for the number based pattern in a pattern word list to see which words match the pattern. I call these the "Fixed Length Patterns".

Common pattern words worth memorizing:

```
1231    123142    12342
that    people    which

122     12343     123314
all     there     little
off     these
see     where
too
```

#### Floating Length Patterns

While researching pattern words I came across a few other pattern types that are useful also. [Codasaurus.com](http://www.codasaurus.com/pattern.htm) mentioned a shorted version of the above numeric pattern that only shows the repeated characters. I call these the "Floating Length Patterns". These are great when you don't know the full length of the word. First calculate the pattern as above. Then, starting at the front, write down all the positions of the first duplicated letter. Continue this process for all other duplicated letters. This form can be useful when you don't know the exact length of a word or when multiple word endings are possible. The above examples would be as follows using this method:

```
14      1426      25
that    people    which

23      35        1534
all     there     little
off     these
see     where
too
```

#### High/Low Frequency Patterns

In the comments on my article [Social Cryptogram Solving](/2009/02/01/social-cryptogram-solving/#comment-7), BOATTAIL mentioned patterns that looked at the frequency of letters. Using a frequency table you can determine which letters are high and low frequency letters. Then replace the letters with H for high and L for low. For English, the high frequency letters are ETAONIRSH. The examples from above would look as follows:

```
HHHH    LHHLLH    LHHLH
that    people    which

HLL     HHHHH     LHHHLH
all     there     little
off     these
see     where
too
```

Searching for Pattern Words
---------------------------

You can find pattern word lists on the internet by just searching for them. Most of the time they are a little hard to handle for beginners because they usually come in raw text files and they generally just contain the fixed length patterns. I've created a special website that will allow you to search for any of the patterns discussed above in a few different word lists. At the moment I have three different word lists that are searchable. They each have their own strengths and weaknesses. You can access it at [http://tools.codepenguin.com/patterns/](http://tools.codepenguin.com/patterns/). The page has instructions for how to use each of the different patterns above and how to apply word filters to weed out the possibilities.

Depending on the word list used, pattern words become less useful simply because they return too many words. That is where the word filters come into play. With word filters, you can shrink the matches for the patterns down to ones that fit your needs. For example: if you look up the pattern 1426 (for people) in the ENABLE2K word list, it returns 130 words. If you know that the first and fourth letters are "p" you can use the filter "p..p\*" the matches drop down to 24 words. 24 is much more manageable and realistic.

If you find my pattern search tool useful, [please let me know](/contact-me/). I'm always open for suggestions on how to improve my tools and for new ideas.

Conclusion
----------

All the different kinds of patterns give you a lot of techniques to aid in solving Aristocrats and Patristocrats. You can get quite far by using them. Pattern word lists are available on the internet and there are some published books that have them too. They are an essential tool for any solver's tool box.