﻿// See https://aka.ms/new-console-template for more information
using FluentColorConsole;

Console.WriteLine("Hello, World!");

var showMessage = new ShowMessage();
showMessage.WriteLine();

var textLine = ColorConsole.WithBlueText;
textLine.WriteLine("My text blue");