# UnlayerCache
This is a cache proxy for some methods of the Unlayer API.

[Unlayer](https://unlayer.com/) is a service that allows you to easily build responsive email templates. The service is being typically used to create user-facing emails.

Clients may want to perform caching on some aspects of the Unlayer API, namely the endpoints that allow the retrieval and rendering of a template. This may be desirable in order to reduce calls to Unlayer or to use the cache when the Unlayer API is unavailable. Unlayer Cache is a .Net web API that acts as a intelligent pass-through proxy to cache the results of these two calls. The service uses [AWS Dynamo](https://aws.amazon.com/dynamodb/) tables as a cache and can be deployed either on an [EC2](https://aws.amazon.com/ec2/) or as a [lambda](https://aws.amazon.com/lambda/) docker image reachable via [API Gateway](https://aws.amazon.com/api-gateway/). 
