// Define and export User class
export class User {
    constructor(user_name, user_password) {
        if (User.instance) {
            return User.instance;
        }

        if (localStorage.getItem("user_name") && localStorage.getItem("user_password")) {
            // If data exists, load it from localStorage
            this.user_name = localStorage.getItem("user_name");
            this.user_password = localStorage.getItem("user_password");
            this.user_id = localStorage.getItem("user_id");
        } else {
            // Otherwise, set the provided user data and save to localStorage
            this.user_name = user_name;
            this.user_password = user_password;
            localStorage.setItem("user_name", user_name);
            localStorage.setItem("user_password", user_password);
            localStorage.setItem("user_id", this.user_id)
        }

        User.instance = this; // Ensure singleton pattern
    }

    saveData() {
        localStorage.setItem("user_name", this.user_name);
        localStorage.setItem("user_password", this.user_password);
        localStorage.setItem("user_id", this.user_id);
    }

    getName() {
        return this.user_name;
    }

    setName(nName) {
        this.user_name = nName;

        this.saveData();//Must use this
    }

    getPassword() {
        return this.user_password;
    }
}