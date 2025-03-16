package project.moto.Repository;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import project.moto.Domain.Entity;
import project.moto.Utils.JdbcUtils;

import java.util.*;

abstract public class AbstractDatabaseRepository<ID, E extends Entity<ID>> implements Repository<ID,E> {
    protected final JdbcUtils dbUtils;
    protected static final Logger logger = LogManager.getLogger();
    protected Map<ID,E> data;

    public AbstractDatabaseRepository(Properties props) {
        this.dbUtils = new JdbcUtils(props);
        this.data = new HashMap<>();
        Load();
    }
    public Map<ID,E> findAll(){
        return data;
    }

    abstract public Optional<E> findOne(ID id);
    abstract public Optional<E> save(E entity);
    abstract public Optional<E> delete(ID id);
    abstract public Optional<E> update(E entity);
    abstract protected void Load();

}
