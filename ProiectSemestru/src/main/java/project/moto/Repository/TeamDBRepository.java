package project.moto.Repository;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import project.moto.Utils.JdbcUtils;
import project.moto.Domain.Team;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.Properties;

public class TeamDBRepository implements Repository<Integer, Team>{
    private JdbcUtils dbUtils;

    private static final Logger logger = LogManager.getLogger();

    public TeamDBRepository(Properties props) {
        this.dbUtils = new JdbcUtils(props);
        logger.info("Initialized TeamDBRepository with properties: " + props);
    }

    @Override
    public Optional<Team> findOne(Integer integer) {
        logger.info("Finding Team by ID: " + integer);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Team where id = ?")) {
            preStmt.setInt(1, integer);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("id");
                    String name = result.getString("Name");
                    Team team = new Team(name);
                    team.setId(id);
                    logger.traceExit();
                    return Optional.of(team);
                }
            }
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }
        logger.traceExit();
        return Optional.empty();
    }

    @Override
    public Iterable<Team> findAll() {
        logger.info("Find all tasks");
        Connection con = dbUtils.getConnection();
        List<Team> teams = new ArrayList<>();
        try (var preStmt = con.prepareStatement("select * from Team")) {
            try (var result = preStmt.executeQuery()) {
                while (result.next()) {
                    int id = result.getInt("id");
                    String name = result.getString("Name");
                    Team team = new Team(name);
                    team.setId(id);
                    teams.add(team);
                }
            }
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }
        logger.traceExit(teams.size());
        return teams;
    }

    @Override
    public Optional<Team> save(Team entity) {
        logger.traceEntry("saving team {} ", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("insert into Team (Id,Name) values (?,?)")) {
            preStmt.setInt(1, entity.getId());
            preStmt.setString(2, entity.getName());
            preStmt.executeUpdate();
        } catch (Exception e) {
            logger.error(e);
            return Optional.of(entity);
        }
        logger.traceExit();
        return Optional.empty();
    }


    @Override
    public Optional<Team> delete(Integer integer) {
        logger.traceEntry("deleting team {} ", integer);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("delete from Team where id = ?")) {
            preStmt.setInt(1, integer);
            preStmt.executeUpdate();
        } catch (Exception e) {
            logger.error(e);
            Team t = new Team("");
            t.setId(integer);
            return Optional.of(t);
        }
        logger.traceExit();
        return Optional.empty();
    }

    @Override
    public Optional<Team> update(Team entity) {
        logger.traceEntry("Updating team {} ", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("update Team set Name = ? where id = ?")) {
            preStmt.setString(1, entity.getName());
            preStmt.setInt(2, entity.getId());
            preStmt.executeUpdate();
        } catch (Exception e) {
            logger.error(e);
            return Optional.of(entity);
        }
        logger.traceExit();
        return Optional.empty();
    }
}
