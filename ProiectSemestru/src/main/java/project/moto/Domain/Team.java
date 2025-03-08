package project.moto.Domain;

public class Team extends Entity<Long> {
    private String name;

    public Team(String name) {
        this.name = name;
    }

    public String getName() {
        return name;
    }
}
