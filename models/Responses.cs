using Newtonsoft.Json;

namespace FocusAI.Backend.Models {
    public class StatusResponse {
        
        public string status;

        public StatusResponse(string status) {
            this.status = "success";
        }
    }

    public class JWTResponse {
        
        public string jwt;

        public JWTResponse(string jwt) {
            this.jwt = jwt;
        }
    }


    public class ListResponse<T> {
        
        public List<T> result;

        public ListResponse(List<T> items) {
            this.result = items;
        }
    }

}