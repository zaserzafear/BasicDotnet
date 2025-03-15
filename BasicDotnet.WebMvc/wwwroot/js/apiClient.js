const apiClient = {
    request: async function (method, url, data = null) {
        try {
            const options = {
                method: method,
                headers: {
                    "Content-Type": "application/json"
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

    get: function (url) {
        return this.request("GET", url);
    },

    post: function (url, data) {
        return this.request("POST", url, data);
    },

    put: function (url, data) {
        return this.request("PUT", url, data);
    },

    delete: function (url) {
        return this.request("DELETE", url);
    }
};
