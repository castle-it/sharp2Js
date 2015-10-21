# sharp2Js
[![Build Status](https://travis-ci.org/castle-it/sharp2Js.svg?branch=master)](https://travis-ci.org/castle-it/sharp2Js)

Simple library to convert your C# POCOs/DTOs to javascript objects

At Castle we do a lot of REST based back and forth between our ASP.Net Web API Services and our javascript front end.
Sometimes our models are complex and are simply labor intensive to type out twice, and it's hard to keep in sync when changes are made.

Why not just work with JSON directly?  We do, for smaller packages, but sometimes there's just no good replacement
for a proper object model.

sharp2Js will create javascript objects that mirror your C# POCO classes, and you can easily redirect that output to T4 templates like we do.

