const apiClient = {
    request: async function (method, url, data = null, headers = {}) {
        try {
            const options = {
                method: method,
                headers: {
                    "Content-Type": "application/json",
                    ...headers // Add custom headers here
                }
            };

            if (data) {
                options.body = JSON.stringify(data);
            }

            const response = await fetch(url, options);

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `Error ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error("API Error:", error.message);
            throw error;
        }
    },

    get: function (url, headers = {}) {
        return this.request("GET", url, null, headers);
    },

    post: function (url, data, headers = {}) {
        return this.request("POST", url, data, headers);
    },

    put: function (url, data, headers = {}) {
        return this.request("PUT", url, data, headers);
    },

    delete: function (url, headers = {}) {
        return this.request("DELETE", url, null, headers);
    }
};
