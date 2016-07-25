using PokemonGoDesktop.API.Common;
using PokemonGoDesktop.API.Proto;
using PokemonGoDesktop.API.Proto.Services;
using ProjectMew;

public static class RequestEnvelopeBuilder
{
    public static RequestEnvelope GetInitialRequestEnvelope(string authToken, AuthType authType, GeoLocation geoLocation, params Request[] requestMessages)
    {
        RequestEnvelope envelope = new RequestEnvelope();

        //Provide the auth type and the oAuth token issued
        envelope.WithAltitude(geoLocation.Altitude)
            .WithLatitude(geoLocation.Latitude)
            .WithLongitude(geoLocation.Longitude)
            .WithRequestID() //RPC ID?
            .WithAuthenticationMessage(authType, authToken);

        //Now we add the requests to the envelope
        foreach (Request r in requestMessages)
            envelope.WithMessage(r);

        return envelope;
    }

    public static RequestEnvelope GetInitialRequestEnvelope(string authToken, AuthType authType, GeoLocation geoLocation)
    {
        RequestEnvelope envelope = new RequestEnvelope();

        //Provide the auth type and the oAuth token issued
        envelope.WithAltitude(geoLocation.Altitude)
            .WithLatitude(geoLocation.Latitude)
            .WithLongitude(geoLocation.Longitude)
            .WithRequestID() //RPC ID?
            .WithAuthenticationMessage(authType, authToken);

        return envelope;
    }

    public static RequestEnvelope GetInitialRequestEnvelope(string authToken, AuthType authType, GeoLocation geoLocation, params RequestType[] requestTypeIds)
    {
        RequestEnvelope envelope = new RequestEnvelope();

        //Provide the auth type and the oAuth token issued
        envelope.WithAltitude(geoLocation.Altitude)
            .WithLatitude(geoLocation.Latitude)
            .WithLongitude(geoLocation.Longitude)
            .WithRequestID() //RPC ID?
            .WithAuthenticationMessage(authType, authToken);

        //Now we generate empty requests and put the ID into them.
        foreach (RequestType r in requestTypeIds)
        {
            Request request = new Request();
            request.RequestType = r;
            envelope.WithMessage(request);
        }

        return envelope;
    }

    public static RequestEnvelope GetRequestEnvelope(AuthTicket authTicket, GeoLocation geoLocation)
    {
        RequestEnvelope envelope = new RequestEnvelope();

        //These requests are sent with our issued AuthTicket
        envelope.WithAltitude(geoLocation.Altitude)
            .WithLatitude(geoLocation.Latitude)
            .WithLongitude(geoLocation.Longitude)
            .WithRequestID() //RPC ID?
            .WithAuthTicket(authTicket);

        return envelope;
    }

    public static RequestEnvelope GetRequestEnvelope(AuthTicket authTicket, GeoLocation geoLocation, RequestType requestType)
    {
        RequestEnvelope envelope = new RequestEnvelope();

        //These requests are sent with our issued AuthTicket
        envelope.WithAltitude(geoLocation.Altitude)
            .WithLatitude(geoLocation.Latitude)
            .WithLongitude(geoLocation.Longitude)
            .WithRequestID() //RPC ID?
            .WithAuthTicket(authTicket);

        //add just the request type
        Request request = new Request();
        request.RequestType = requestType;
        envelope.WithMessage(request);

        return envelope;
    }
}