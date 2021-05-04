// These helpers are proposed helpers for @microsoft/mgt-react

import { equals, prepScopes, Providers, ProviderState } from "@microsoft/mgt-element";
import { useCallback, useEffect, useState } from "react";

export function useIsSignedIn(): [boolean] {
  const [isSignedIn, setIsSignedIn] = useState(false);

  useEffect(() => {
    const updateState = () => {
      let provider = Providers.globalProvider;
      setIsSignedIn(provider && provider.state === ProviderState.SignedIn);
    };

    Providers.onProviderUpdated(updateState);
    updateState();

    return () => {
      Providers.removeProviderUpdatedListener(updateState);
    };
  }, []);

  return [isSignedIn];
}

export interface GetOptions {
  version?: string;
  pollingRate?: number; // TODO - poll the api at this rate - polls delta api when specified as resource
  maxPages?: number; // TODO - follow pages up to the max number
  scopes?: string[];
  type?: "json" | "image";
  dependencies?: unknown[];
}

export function useGet<T = any>(
  resource: string,
  options?: GetOptions
): [T | undefined, boolean, any] {
  const [fullResponse, setResponse] = useState<T>();
  const [error, setError] = useState<any>();
  const [loading, setLoading] = useState(false);

  const [isSignedIn] = useIsSignedIn();

  const loadResponse = useCallback(async (isPolling: boolean = false) => {
    console.log("load response called");
    if (
      !loading &&
      isSignedIn &&
      (!dependencies || dependencies.every((d) => !!d))
    ) {
      setLoading(true);
      console.log("starting request");

      let tempResponse = null;
      try {
        let uri = resource;
        let isDeltaLink = false;

        console.log(fullResponse);
        if (fullResponse && (fullResponse as any)["@odata.deltaLink"]) {
          uri = (fullResponse as any)["@odata.deltaLink"];
          isDeltaLink = true;
        } else {
          isDeltaLink = new URL(
            uri,
            "https://graph.microsoft.com"
          ).pathname.endsWith("delta");
        }

        const provider = Providers.globalProvider;
        const graph = provider.graph; // .forComponent TODO update to take in string
        let request = graph.api(uri).version(version);

        if (scopes && scopes.length) {
          request = request.middlewareOptions(prepScopes(...scopes));
        }

        console.log("making request", uri, isDeltaLink);

        if (type === "json") {
          tempResponse = await request.get();

          if (
            isDeltaLink &&
            fullResponse &&
            Array.isArray((fullResponse as any).value) &&
            Array.isArray(tempResponse.value)
          ) {
            tempResponse.value = (fullResponse as any).value.concat(
              tempResponse.value
            );
          }

          if (!isPolling && !equals(fullResponse, tempResponse)) {
            setResponse(tempResponse);
          }

          // get more pages if there are available
          if (
            tempResponse &&
            Array.isArray(tempResponse.value) &&
            tempResponse["@odata.nextLink"]
          ) {
            let pageCount = 1;
            let page = tempResponse;

            while (
              (pageCount < maxPages ||
                maxPages <= 0 ||
                (isDeltaLink && pollingRate)) &&
              page &&
              page["@odata.nextLink"]
            ) {
              pageCount++;
              const nextResource = page["@odata.nextLink"].split(version)[1];
              page = await graph.client
                .api(nextResource)
                .version(version)
                .get();
              if (page && page.value && page.value.length) {
                page.value = tempResponse.value.concat(page.value);
                tempResponse = page;
                if (!isPolling) {
                  setResponse(tempResponse);
                }
              }
            }
          }
        } else {
          // TODO
          // if (resource.indexOf('/photo/$value') === -1) {
          //   setError('Only /photo/$value endpoints support the image type');
          //   return;
          // }
          // // Sanitizing the resource to ensure getPhotoForResource gets the right format
          // const sanitizedResource = resource.replace('/photo/$value', '');
          // const photoResponse = await getPhotoForResource(graph, sanitizedResource, this.scopes);
          // if (photoResponse) {
          //   response = {
          //     image: photoResponse.photo
          //   };
          // }
        }

        if (!equals(fullResponse, tempResponse)) {
          setResponse(tempResponse);
        }
      } catch (e) {
        setError(e);
      }
      setLoading(false);

      if (tempResponse) {
        setError(null);

        // TODO
        if (pollingRate) {
          setTimeout(async () => {
            console.log("starting polling");
            await loadResponse(true);
          }, pollingRate);
        }
      }
    }
  }, [fullResponse, loading, isSignedIn]);

  // console.log('useGet called', resource, options);

  // default values
  let version = "v1.0";
  let scopes: string[] | null = null;
  let type = "json";
  let maxPages = 1;
  let pollingRate = 0;
  let dependencies: unknown[] | null = null;

  if (options) {
    version = options.version || version;
    scopes = options.scopes || null;
    type = options.type || type;
    maxPages = options.maxPages !== undefined ? options.maxPages : maxPages;
    pollingRate =
      options.pollingRate !== undefined ? options.pollingRate : pollingRate;
    dependencies = options.dependencies || null;
  }

  useEffect(() => {
    loadResponse();

    return () => {
      // stop timers or unsubscribe from events here
    };
    // TODO, figure out why adding the deps or options params here causes the
    // hook to execute constantly
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isSignedIn, resource]);

  // TODO, add function to return array to call when the dev wants to fetch more pages manually
  return [fullResponse, loading, error];
}
