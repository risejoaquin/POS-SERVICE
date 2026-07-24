using System;
using Squirrel;
class Program { static void Main() {
    SquirrelAwareApp.HandleEvents(onInitialInstall: (v, t) => {});
} }
