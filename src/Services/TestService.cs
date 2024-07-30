using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.src.Services;

public class TestService
{
    [ResourceMethod(RequestMethod.GET)]
    public string GetTest(IRequest request)
    {
        return "This is a test of the PirateREST.";
    }
}

