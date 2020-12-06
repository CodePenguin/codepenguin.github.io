---
layout: post
title: Outwitters Sports Network
date: 2013-12-28 18:12:36.000000000 -05:00
type: post
parent_id: '0'
published: true
password: ''
status: publish
tags:
- programming
permalink: "/2013/12/28/outwitters-sports-network/"
---
It has been a while since I posted here so I wanted to give some details to what I've been working on over the last year. About a year and a half ago I discovered an iOS turn-based strategy game called [Outwitters](http://onemanleft.com/games/outwitters/ "Outwitters"). I was immediately hooked. It had enough variation and strategy to keep my interest. I was playing it all the time and recommending it to everyone who would listen.

<iframe width="549" height="309" src="https://www.youtube.com/embed/1Z9R7H-OZb8" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

After you finish a game you have the ability to re-watch the replays and share them with others. This allowed you to learn new strategies or show off a triumphant victory. Unfortunately, you could only share the replays with someone who had an iOS device and had the Outwitters app installed. I wanted to watch the replays on my desktop so that I could easily compare and contrast different strategies against each other and improve my game.

### Outwitters Replay Viewer

I began the process of reverse-engineering the replay protocol (with permission from the original developers) and creating the Outwitters Replay Viewer. I didn't have any help from the original developers about protocols, file formats, or anything like that so I had to figure that all out on my own based on the data I could extract from the game. It took a bit of time to get everything squared away but in the end it worked out great.

<!--more-->
<a name="more" />

The original version of the replay viewer wasn't much to look at but it was functional. The second version had very basic graphics to indicate the types of units. But the 3rd version included all the graphics from the original game, looked the best and was the most functional.


| ![iOS on Device Replay]({{ site.baseurl }}/assets/2013/12/iosreplay.png) |
| iOS on Device Replay |
| ![Replay Viewer Version 1]({{ site.baseurl }}/assets/2013/12/version1.jpg) |
| Replay Viewer Version 1 |
| ![Replay Viewer Version 2]({{ site.baseurl }}/assets/2013/12/version2.jpg) |
| Replay Viewer Version 2 |
| ![Replay Viewer Version 3]({{ site.baseurl }}/assets/2013/12/version3.jpg) |
 | Replay Viewer Version 3 |

### Outwitters Sports Network is born!

Once the replay viewer was completed and people started using it, there began to be a lot of replay data stored on my server and so I started scanning them and creating analytics that allowed you to quickly find specific replays by map, player, unit, etc. This began the transition from just a replay viewer into the [Outwitters Sports Network](http://osn.codepenguin.com "Outwitters Sports Network") (Abbreviated as OSN). There are [statistics](http://osn.codepenguin.com/statistics "OSN Statistics"), [weekly top ranked player lists](http://osn.codepenguin.com/ranks/view/latest "OSN Top Player Ranks"), [leaderboards](http://osn.codepenguin.com/leaderboards "OSN Leaderboards"), [popular replays](http://osn.codepenguin.com/replays/lists/popular "OSN Popular Replays") and [much more](http://osn.codepenguin.com/replays "OSN"). According to Google Analytics, from August 2012 to December 2013 there have been 86,237 visits, 17,951 unique visitors, and 355,938 page-views. As of December 28, 1013 there have been 21,872 user submitted replays.

| ![OSN Final Replay Viewer]({{ site.baseurl }}/assets/2013/12/osn_final.png) |
| OSN Final Replay Viewer |

| ![OSN Website]({{ site.baseurl }}/assets/2013/12/osn_final2.png) |
| OSN Website |

### Technical Details

The replay viewer was programmed in [Processing](http://www.processing.org/ "Processing") and then loaded on the website using [Processing.js](http://processingjs.org/ "Processing JS"). Processing didn't originally support JSON data so I had to use the compiled add-on library and examples at [Jer Thorp's Blog article "Processing, JSON & The New York Times"](http://blog.blprnt.com/blog/blprnt/processing-json-the-new-york-times "Processing, JSON & The New York Times"). Processing.js on the other hand does not support any java add-on libraries like Processing does so I ended up creating a special JavaScript based add-on that would supply Processing.js a library with the same footprint as the original java JSON library. I found a wonderful reference guide on Processing.js's website called [Pomax's Guide to Processing.js](http://processingjs.org/articles/PomaxGuide.html "Pomax's guide to Processing.js") that was invaluable in getting Processing.js and JavaScript to work together. This allowed Processing and Processing.js to use the exact same Processing based code so I didn't have to support two separate code bases.

The Outwitters Sports Network Website utilizes the following technologies:

*   [PHP](http://php.net/ "PHP") and [CodeIgniter](http://ellislab.com/codeigniter "CodeIgniter") for the backend processing and website
*   [Bootstrap](http://getbootstrap.com/ "Bootstrap") for the majority of styles on the website
*   [JQuery](http://jquery.com/ "JQuery") for the powerful dynamic web interface
*   [Handlebars](http://handlebarsjs.com/ "Handlebars") for client-side templating
*   [Highcharts](http://www.highcharts.com/ "Highcharts") for client-side charts on the statistics page
*   [MySQL](http://www.mysql.com/ "MySQL") for data storage of statistics and searchable data
*   [JSON](http://www.json.org/ "JSON") text files for storage of full replay data files

### Summary

It was definitely a long ride but a pretty enjoyable one. The final product ended up being way different than I had originally planned but I think it came out great. Especially, since it was originally just a proof of concept to see if I could do it. The [Outwitters Sports Network](http://osn.codepenguin.com "Outwitters Sports Network") is still going strong and still gets quite a few visitors. It is currently in maintenance mode as I've pretty much added all the features that I really wanted to be there. I still update the top player ranks every week and keep things running smoothly.