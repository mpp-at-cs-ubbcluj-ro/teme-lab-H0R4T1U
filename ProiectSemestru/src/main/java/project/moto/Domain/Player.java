package project.moto.Domain;

public class Player extends Entity<Long>{
    private String name;
    private String code;
    private String team;

    public Player(String name, String code, String team) {
        this.name = name;
        this.code = code;
        this.team = team;
    }

    public String getName() {
        return name;
    }

    public String getCode() {
        return code;
    }

    public String getTeam() {
        return team;
    }
}
