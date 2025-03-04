package ubb.scs.map.model;

public interface Identifiable<Tid> {
    Tid getID();
    void setID(Tid id);
}
