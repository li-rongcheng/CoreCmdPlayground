﻿using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ActorPlayground.Commands.Orleans
{

    class OrleansCommand
    {
        public async Task Silo()
        {
            await new MySilo().RunAsync();
        }

        public async Task Client()
        {
            await new MyClient().RunAsync();
        }
    }
}
