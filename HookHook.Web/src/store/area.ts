import { authHeader } from ".";

const area = {
    namespaced: true,
    actions: {
        async getServices(_: any) {
            const res = await fetch("/api/area/getservices", {
                method: 'GET',
                headers: authHeader()
            })
            if (res.status === 500) {
                return { error: "Backend unavailable" };
            }
            const contentType = res.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                const json = await res.json();

                return json;
            }
            if (contentType && contentType.indexOf("application/problem+json") !== 1) {
                const { error, errors } = await res.json();
                return { error, errors };
            }
            return {};
        },
        async createAreaRequest(_:any, json: any) {
            const res = await fetch("/api/area/create", {
                method: 'POST',
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(json)
            })
            if (res.status === 500) {
                return { error: "Backend unavailable" };
            }
            const contentType = res.headers.get("content-type");
            if (contentType && (contentType.indexOf("application/json") !== -1 || contentType.indexOf("application/problem+json") !== -1)) {
                const { error, errors } = await res.json();
                return { error, errors };
            }
            return {};
        }
    }
}

export default area